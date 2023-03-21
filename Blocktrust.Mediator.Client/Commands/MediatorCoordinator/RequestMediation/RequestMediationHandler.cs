namespace Blocktrust.Mediator.Client.Commands.MediatorCoordinator.RequestMediation;

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
using Blocktrust.Mediator.Common.Models.OutOfBand;
using Blocktrust.Mediator.Common.Protocols;
using Blocktrust.PeerDID.DIDDoc;
using Blocktrust.PeerDID.PeerDIDCreateResolve;
using Blocktrust.PeerDID.Types;
using Common.Models.ProblemReport;
using FluentResults;
using MediatR;

public class RequestMediationHandler : IRequestHandler<RequestMediationRequest, Result<RequestMediationResponse>>
{
    private readonly HttpClient _httpClient;
    private readonly IDidDocResolver _didDocResolver;
    private readonly ISecretResolver _secretResolver;

    public RequestMediationHandler(HttpClient httpClient, IDidDocResolver didDocResolver, ISecretResolver secretResolver)
    {
        _httpClient = httpClient;
        _didDocResolver = didDocResolver;
        _secretResolver = secretResolver;
    }

    public async Task<Result<RequestMediationResponse>> Handle(RequestMediationRequest request, CancellationToken cancellationToken)
    {
        // We decode the peerDID of the mediator from the invitation
        OobModel? remoteDid;
        try
        {
            var decodedInvitation = Encoding.UTF8.GetString(Base64Url.Decode(request.OobInvitation));
            remoteDid = JsonSerializer.Deserialize<OobModel>(decodedInvitation);
            if (!remoteDid!.Type.Equals(ProtocolConstants.OutOfBand2Invitation, StringComparison.CurrentCultureIgnoreCase))
            {
                return Result.Fail("Invalid invitation type");
            }
        }
        catch (Exception e)
        {
            return Result.Fail($"Unable to parse OutOfBand Invitation: {e}");
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

        var didComm = new DidComm(_didDocResolver, _secretResolver);

        // We pack the message and encrypt it for the mediator
        var packResult = await didComm.PackEncrypted(
            new PackEncryptedParamsBuilder(mediateRequestMessage, to: remoteDid.From)
                .From(request.LocalDid)
                .ProtectSenderId(false)
                .BuildPackEncryptedParams()
        );

        // We send the message to the mediator
        var endpoint = invitationPeerDidDocResult.Value.Services?.FirstOrDefault()?.ServiceEndpoint;
        if (endpoint is null)
        {
            return Result.Fail("Unable to identify endpoint of mediator");
        }

        var endpointUri = new Uri(endpoint);
        HttpResponseMessage response;
        try
        {
            response = await _httpClient.PostAsync(endpointUri, new StringContent(packResult.PackedMessage, Encoding.UTF8, MessageTyp.Encrypted), cancellationToken);
        }
        catch (HttpRequestException ex)
        {
            return Result.Fail($"Connection could not be established: {ex.Message}");
        }

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return Result.Fail("Connection could not be established");
        }
        else if (!response.IsSuccessStatusCode)
        {
            return Result.Fail("Unable to initiate connection: " + response.StatusCode);
        }

        var content = await response.Content.ReadAsStringAsync(cancellationToken);

        var unpackResult = await didComm.Unpack(
            new UnpackParamsBuilder(content)
                .SecretResolver(_secretResolver)
                .BuildUnpackParams());

        if (unpackResult.IsFailed)
        {
            return unpackResult.ToResult();
        }

        if (unpackResult.Value.Message.Type == ProtocolConstants.CoordinateMediation2Deny)
        {
            return Result.Ok(new RequestMediationResponse());
        }
        else
        {
            if (unpackResult.Value.Message.Type == ProtocolConstants.CoordinateMediation2Grant)
            {
                if (!unpackResult.Value.Message.Body.ContainsKey("routing_did"))
                {
                    //TODO a bit unclear what to do here, if the routing_did is not present. Fall back to the already known DID of the mediator?    
                    return Result.Fail("No routing_did present in grant message");
                }

                var from = unpackResult.Value.Message.From;
                if (from is null && unpackResult.Value.Message.FromPrior is not null)
                {
                    // We have a new DID from the mediator, caused by a DID rotation. This is expected.
                    from = unpackResult.Value.Message.FromPrior.Sub;
                    var fromOld = unpackResult.Value.Message.FromPrior.Iss;
                    if (!fromOld.Equals(remoteDid.From))
                    {
                        // The old DID of the mediator does not match the one we have in the invitation. This is unexpected.
                        return Result.Fail("Unexpected DID rotation");
                    }
                }

                return Result.Ok(new RequestMediationResponse(from!, endpointUri, unpackResult.Value.Message.Body["routing_did"]!.ToString()));
            }
            
            if (unpackResult.Value.Message.Type == ProtocolConstants.ProblemReport)
            {
                if (unpackResult.Value.Message.Pthid != null)
                {
                    var problemReport = ProblemReport.Parse(unpackResult.Value.Message.Body, unpackResult.Value.Message.Pthid);
                    if (problemReport.IsFailed)
                    {
                        return Result.Fail("Error parsing the problem report of the mediator");
                    }

                    return Result.Ok(new RequestMediationResponse(problemReport.Value));
                }
                return Result.Fail("Error parsing the problem report of the mediator. Missing parent-thread-id");
            }

            return Result.Fail("Error: Unexpected message response type");
        }
    }
}