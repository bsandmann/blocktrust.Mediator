namespace Blocktrust.Mediator.Client.Commands.MediatorCoordinator.InquireMediation;

using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Blocktrust.Common.Converter;
using Blocktrust.Common.Resolver;
using Blocktrust.DIDComm;
using Blocktrust.DIDComm.Common.Types;
using Blocktrust.DIDComm.Message.Messages;
using Blocktrust.DIDComm.Model.PackEncryptedParamsModels;
using Blocktrust.DIDComm.Model.UnpackParamsModels;
using Blocktrust.Mediator.Common.Commands.CreatePeerDid;
using Blocktrust.Mediator.Common.Models.OutOfBand;
using Blocktrust.Mediator.Common.Protocols;
using Blocktrust.PeerDID.DIDDoc;
using Blocktrust.PeerDID.PeerDIDCreateResolve;
using Blocktrust.PeerDID.Types;
using FluentResults;
using MediatR;

public class InquireMediationHandler : IRequestHandler<InquireMediationRequest, Result<InquireMediationResponse>>
{
    private readonly IMediator _mediator;
    private readonly HttpClient _httpClient;
    private readonly IDidDocResolver _didDocResolver;
    private readonly ISecretResolver _secretResolver;

    public InquireMediationHandler(IMediator mediator, HttpClient httpClient, IDidDocResolver didDocResolver, ISecretResolver secretResolver)
    {
        _mediator = mediator;
        _httpClient = httpClient;
        _didDocResolver = didDocResolver;
        _secretResolver = secretResolver;
    }

    public async Task<Result<InquireMediationResponse>> Handle(InquireMediationRequest request, CancellationToken cancellationToken)
    {
        // For the communication with the mediator we need a new peerDID
        var localDid = await _mediator.Send(new CreatePeerDidRequest(), cancellationToken);
        if (localDid.IsFailed)
        {
            return Result.Fail($"Invalid arguments for peerDID creation: {localDid.Errors.First().Message}");
        }

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

        // We create the message to send to the mediator
        // See: https://didcomm.org/mediator-coordination/2.0/
        // The special format of the return_route header is required by the python implementation of the roots mediator
        var returnRoute = new JsonObject() { new KeyValuePair<string, JsonNode?>("return_route", "all") };
        var mediateRequestMessage = new MessageBuilder(
                id: Guid.NewGuid().ToString(),
                type: ProtocolConstants.CoordinateMediation2Request,
                body: new Dictionary<string, object>()
            )
            .customHeader("custom_headers", new List<JsonObject>() { returnRoute })
            .build();


        var invitationPeerDidResult = PeerDidResolver.ResolvePeerDid(new PeerDid(remoteDid.From), VerificationMaterialFormatPeerDid.Jwk);
        if (invitationPeerDidResult.IsFailed)
        {
            return Result.Fail("Unable to parse peerDID from invitation");
        }

        var invitationPeerDidDocResult = DidDocPeerDid.FromJson(invitationPeerDidResult.Value);
        if (invitationPeerDidDocResult.IsFailed)
        {
            return Result.Fail("Unable to parse peerDID from invitation");
        }

        //This is a rather trashy implementation
        var zippedAgreementKeysAndSecrets = localDid.Value.PrivateAgreementKeys
            .Zip(localDid.Value.DidDoc.KeyAgreements
                .Select(p => p.Id), (secret, kid) => new { secret = secret, kid = kid });
        foreach (var zip in zippedAgreementKeysAndSecrets)
        {
            zip.secret.Kid = zip.kid;
            _secretResolver.AddKey(zip.kid, zip.secret);
        }
        
        var zippedAuthenticationKeysAndSecrets = localDid.Value.PrivateAuthenticationKeys
            .Zip(localDid.Value.DidDoc.Authentications
                .Select(p => p.Id), (secret, kid) => new { secret = secret, kid = kid });
        foreach (var zip in zippedAuthenticationKeysAndSecrets)
        {
            zip.secret.Kid = zip.kid;
            _secretResolver.AddKey(zip.kid, zip.secret);
        }


        var didComm = new DidComm(_didDocResolver, _secretResolver);

        // We pack the message and encrypt it for the mediator
        var packResult = didComm.PackEncrypted(
            new PackEncryptedParamsBuilder(mediateRequestMessage, to: remoteDid.From)
                .From(localDid.Value.PeerDid.Value)
                .ProtectSenderId(false)
                .BuildPackEncryptedParams()
        );

        // We send the message to the mediator
        var endpoint = invitationPeerDidDocResult.Value.Services?.First().ServiceEndpoint;
        var endpointUri = new Uri(endpoint);
        var response = await _httpClient.PostAsync(endpointUri, new StringContent(packResult.PackedMessage, Encoding.UTF8, MessageTyp.Encrypted), cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return Result.Fail("Connection could not be established");
        }
        else if (!response.IsSuccessStatusCode)
        {
            return Result.Fail("Unable to initiate connection: " + response.StatusCode);
        }

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        
        var unpackResult = didComm.Unpack(
            new UnpackParamsBuilder(content)
                .SecretResolver(_secretResolver)
                .BuildUnpackParams());

        if (unpackResult.IsFailed)
        {
            return unpackResult.ToResult();
        }

        if (unpackResult.Value.Message.Type == ProtocolConstants.CoordinateMediation2Deny)
        {
            return Result.Ok(new InquireMediationResponse());
        }
        else if (unpackResult.Value.Message.Type == ProtocolConstants.CoordinateMediation2Grant)
        {
            if (!unpackResult.Value.Message.Body.ContainsKey("routing_did"))
            {
                //TODO a bit unclear what to do here, if the routing_did is not present. Fall back to the already known DID of the mediator?    
                return Result.Fail("No routing_did present in grant message");
            }

            return Result.Ok(new InquireMediationResponse(unpackResult.Value.Message.Body["routing_did"].ToString()));
        }

        return Result.Fail("Unknown error: ${content} ");
    }
}