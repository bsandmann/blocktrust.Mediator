namespace Blocktrust.Mediator.Client.Commands.InitiateMediate;

using System.Text;
using System.Text.Json;
using Blocktrust.Common.Converter;
using Common.Commands.CreatePeerDid;
using Common.Protocols;
using DIDComm_v2;
using DIDComm_v2.Common.Types;
using DIDComm_v2.DidDocs;
using DIDComm_v2.Message.Messages;
using DIDComm_v2.Model.PackEncryptedParamsModels;
using DIDComm_v2.Secrets;
using FluentResults;
using MediatR;
using PeerDID.DIDDoc;
using PeerDID.PeerDIDCreateResolve;
using PeerDID.Types;
using Server.Models;

public class InitiateMediateHandler : IRequestHandler<InitiateMediateRequest, Result<string>>
{
    private IMediator _mediator;

    public InitiateMediateHandler(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task<Result<string>> Handle(InitiateMediateRequest request, CancellationToken cancellationToken)
    {
        // For the communication with the mediator we need a new peerDID
        var peerDid = await _mediator.Send(new CreatePeerDidRequest(), cancellationToken);

        // We decode the peerDID of the mediator from the invitation
        OobModel? invitation;
        try
        {
            var decodedInvitation = Encoding.UTF8.GetString(Base64Url.Decode(request.OobInvitation));
            invitation = JsonSerializer.Deserialize<OobModel>(decodedInvitation);
            if (!invitation.Type.Equals(ProtocolConstants.OutOfBand2Invitation, StringComparison.CurrentCultureIgnoreCase))
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
        var mediateRequestMessage = new MessageBuilder(
                id: Guid.NewGuid().ToString(),
                type: ProtocolConstants.CoordinateMediation2Request,
                body: new Dictionary<string, object>()
            )
            .customHeader("return_route", "all")
            .build();

        //TODO create the DIDDoc resolver
        var p = PeerDidResolver.ResolvePeerDid(peerDid.Value, VerificationMaterialFormatPeerDid.Jwk);
        var peerDidDoc = DidDocPeerDid.FromJson(p);
        var peerDidDocDictionary = peerDidDoc.ToDict();
        var didCommDidDoc = new DidDoc()
        {
            Did = peerDidDoc.Did,
            KeyAgreements = new List<string>(),
            Authentications = new List<string>(),
            VerificationMethods = new List<VerificationMethod>(),
            DidCommServices = new List<DidCommService>()
        };

        //TODO create the secret resolver


        var didDocResolver = new DIDDocResolverInMemory(new Dictionary<string, DidDoc>() { { peerDid.Value.Value, didCommDidDoc } });
        var secretResolver = new SecretResolverInMemory(new Dictionary<string, Secret>()
        {
            {
                "abc", new Secret(
                    kid: "",
                    type: VerificationMethodType.JSON_WEB_KEY_2020,
                    verificationMaterial: new VerificationMaterial()
                )
            }
        });

        var didComm = new DIDComm(didDocResolver, secretResolver);

        // We pack the message and encrypt it for the mediator
        var packResult = didComm.PackEncrypted(
            new PackEncryptedParamsBuilder(mediateRequestMessage, invitation.From)
                .From(peerDid.Value.Value)
                .BuildPackEncryptedParams()
        );

        // We send the message to the mediator
        return "";
    }
}