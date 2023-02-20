namespace Blocktrust.Mediator.Client.Commands.InitiateMediate;

using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Blocktrust.Common.Converter;
using Blocktrust.Common.Models.DidDoc;
using Blocktrust.Common.Models.Secrets;
using Common;
using Common.Commands.CreatePeerDid;
using Common.Models.OutOfBand;
using Common.Protocols;
using DIDComm;
using DIDComm.Common.Types;
using DIDComm.Message.Messages;
using DIDComm.Model.PackEncryptedParamsModels;
using DIDComm.Model.UnpackParamsModels;
using DIDComm.Secrets;
using FluentResults;
using MediatR;
using PeerDID.DIDDoc;
using PeerDID.PeerDIDCreateResolve;
using PeerDID.Types;

public class InitiateMediateHandler : IRequestHandler<InitiateMediateRequest, Result>
{
    private readonly IMediator _mediator;
    private readonly HttpClient _httpClient;

    public InitiateMediateHandler(IMediator mediator, HttpClient httpClient)
    {
        _mediator = mediator;
        _httpClient = httpClient;
    }

    public async Task<Result> Handle(InitiateMediateRequest request, CancellationToken cancellationToken)
    {
        // For the communication with the mediator we need a new peerDID
        var localDid = await _mediator.Send(new CreatePeerDidRequest(), cancellationToken);

        // We decode the peerDID of the mediator from the invitation
        OobModel? remoteDid;
        try
        {
            var decodedInvitation = Encoding.UTF8.GetString(Base64Url.Decode(request.OobInvitation));
            remoteDid = JsonSerializer.Deserialize<OobModel>(decodedInvitation);
            if (!remoteDid.Type.Equals(ProtocolConstants.OutOfBand2Invitation, StringComparison.CurrentCultureIgnoreCase))
            {
                return Result.Fail("Invalid invitation type");
            }
        }
        catch (Exception e)
        {
            return Result.Fail("Unable to parse OutOfBand Invitation");
        }

        var t = new JsonObject() { new KeyValuePair<string, JsonNode?>("return_route", "all") };

        // We create the message to send to the mediator
        // See: https://didcomm.org/mediator-coordination/2.0/
        var mediateRequestMessage = new MessageBuilder(
                id: Guid.NewGuid().ToString(),
                type: ProtocolConstants.CoordinateMediation2Request,
                body: new Dictionary<string, object>()
            )
            // .customHeader("return_route", "all")
            // .customHeader("custom_headers","""[{'return_route':'all'}]""")
            .customHeader("custom_headers", new List<JsonObject>() { t })
            .build();


        // We need to fill the DIDDoc resolver with the peerDID of the mediator
        var didDocResolver = new SimpleDidDocResolver(new Dictionary<string, DidDoc>());
        var invitationPeerDidString = PeerDidResolver.ResolvePeerDid(new PeerDid(remoteDid.From), VerificationMaterialFormatPeerDid.Jwk);
        var invitationPeerDidDoc = DidDocPeerDid.FromJson(invitationPeerDidString);
        var combinedVerificationMethodsOfInvitation = invitationPeerDidDoc.Authentications.Concat(invitationPeerDidDoc.KeyAgreements);

        didDocResolver.AddDoc(new DidDoc()
        {
            Did = invitationPeerDidDoc.Did,
            KeyAgreements = invitationPeerDidDoc.KeyAgreements.Select(p => p.Id).ToList(),
            Authentications = invitationPeerDidDoc.Authentications.Select(p => p.Id).ToList(),
            VerificationMethods = combinedVerificationMethodsOfInvitation.Select(p => new VerificationMethod(
                id: p.Id,
                type: VerificationMethodType.JsonWebKey2020,
                verificationMaterial: new VerificationMaterial(
                    format: VerificationMaterialFormat.Jwk,
                    value: JsonSerializer.Serialize((PeerDidJwk)p.VerMaterial.Value)),
                controller: p.Controller
            )).ToList(),
            Services = new List<Service>()
        });

        // And also add in the peerDID we just created for the communication with the mediator
        var combinedVerificationMethodsOfSender = localDid.Value.DidDoc.Authentications.Concat(localDid.Value.DidDoc.KeyAgreements);
        didDocResolver.AddDoc(new DidDoc()
        {
            Did = localDid.Value.DidDoc.Did,
            KeyAgreements = localDid.Value.DidDoc.KeyAgreements.Select(p => p.Id).ToList(),
            Authentications = localDid.Value.DidDoc.Authentications.Select(p => p.Id).ToList(),
            VerificationMethods = combinedVerificationMethodsOfSender.Select(p => new VerificationMethod(
                id: p.Id,
                type: VerificationMethodType.JsonWebKey2020,
                verificationMaterial: new VerificationMaterial(
                    format: VerificationMaterialFormat.Jwk,
                    value: JsonSerializer.Serialize((PeerDidJwk)p.VerMaterial.Value)),
                controller: p.Controller
            )).ToList(),
            Services = new List<Service>(),
        });

        //This is a rather trashy implementation
        var secretResolver = new SecretResolverInMemory(new Dictionary<string, Secret>());
        var zippedAgreementKeysAndSecrets = localDid.Value.PrivateAgreementKeys
            .Zip(localDid.Value.DidDoc.KeyAgreements
                .Select(p => p.Id), (secret, kid) => new { secret = secret, kid = kid });
        foreach (var zip in zippedAgreementKeysAndSecrets)
        {
            // var fullKid = $"{newlyCreatedPeerDid.Value.DidDoc.Did}#{zip.kid}";
            var onlykid = zip.kid;
            zip.secret.Kid = zip.kid;
            secretResolver.AddKey(onlykid, zip.secret);
        }

        var zippedAuthenticationKeysAndSecrets = localDid.Value.PrivateAuthenticationKeys
            .Zip(localDid.Value.DidDoc.Authentications
                .Select(p => p.Id), (secret, kid) => new { secret = secret, kid = kid });
        foreach (var zip in zippedAuthenticationKeysAndSecrets)
        {
            // var fullKid = $"{newlyCreatedPeerDid.Value.DidDoc.Did}#{zip.kid}";
            var onlykid = zip.kid;
            zip.secret.Kid = zip.kid;
            secretResolver.AddKey(onlykid, zip.secret);
        }


        var didComm = new DidComm(didDocResolver, secretResolver);

        // We pack the message and encrypt it for the mediator
        var packResult = didComm.PackEncrypted(
            new PackEncryptedParamsBuilder(mediateRequestMessage, to: remoteDid.From)
                .From(localDid.Value.PeerDid.Value)
                .ProtectSenderId(false)
                .BuildPackEncryptedParams()
        );

        // We send the message to the mediator
        var endpoint = invitationPeerDidDoc.Services?.First().ServiceEndpoint;
        var endpointUri = new Uri(endpoint);
        var response = await _httpClient.PostAsync(endpointUri, new StringContent(packResult.PackedMessage, Encoding.UTF8, MessageTyp.Encrypted), cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
        }

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        var unpackResult = didComm.Unpack(
            new UnpackParamsBuilder(content)
                .SecretResolver(secretResolver)
                .BuildUnpackParams());
        
       
       //TODO refinement
       if (unpackResult.Message.Type == "https://didcomm.org/coordinate-mediation/2.0/mediate-grant")
       {
           return Result.Ok();
       }

       return Result.Fail("didn't work :(");
    }
}