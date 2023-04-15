namespace Blocktrust.Mediator.Client.Commands.PrismConnect.ProcessOobInvitationAndConnect;

using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Nodes;
using Blocktrust.Common.Resolver;
using Blocktrust.DIDComm;
using Blocktrust.DIDComm.Common.Types;
using Blocktrust.DIDComm.Message.Messages;
using Blocktrust.DIDComm.Model.PackEncryptedParamsModels;
using Blocktrust.DIDComm.Model.UnpackParamsModels;
using Blocktrust.Mediator.Client.Commands.Pickup.DeliveryRequest;
using Blocktrust.Mediator.Client.Commands.Pickup.MessageReceived;
using Blocktrust.Mediator.Common;
using Blocktrust.Mediator.Common.Models.ProblemReport;
using Blocktrust.Mediator.Common.Protocols;
using FluentResults;
using ForwardMessage;
using MediatR;
using PeerDID.DIDDoc;
using PeerDID.Types;

/// <summary>
/// Assumes that we got a oob-invitation from a prism agent.
/// This requests starts the process of sending connect-request to prism-agent and processes the answer from the agent
/// </summary>
public class PrismConnectHandler : IRequestHandler<PrismConnectRequest, Result<PrismConnectResponse>>
{
    private readonly HttpClient _httpClient;
    private readonly IDidDocResolver _didDocResolver;
    private readonly ISecretResolver _secretResolver;
    private readonly IMediator _mediator;

    public PrismConnectHandler(HttpClient httpClient, IDidDocResolver didDocResolver, ISecretResolver secretResolver, IMediator mediator)
    {
        _httpClient = httpClient;
        _didDocResolver = didDocResolver;
        _secretResolver = secretResolver;
        _mediator = mediator;
    }

    public async Task<Result<PrismConnectResponse>> Handle(PrismConnectRequest request, CancellationToken cancellationToken)
    {
        // The special format of the return_route header is required by the python implementation of the roots mediator
        var returnRoute = new JsonObject() { new KeyValuePair<string, JsonNode?>("return_route", "all") };
        var body = new Dictionary<string, object>();
        body.Add("goal_code", GoalCodes.PrismConnect);
        body.Add("goal", "Connect");
        var acceptList = new List<string>() { "didcomm/v2" };
        body.Add("accept", acceptList);
        var mediateRequestMessage = new MessageBuilder(
                id: Guid.NewGuid().ToString(),
                type: ProtocolConstants.PrismConnectRequest,
                body: body
            )
            .customHeader("custom_headers", new List<JsonObject>() { returnRoute })
            .customHeader("return_route", "all")
            .thid(request.ThreadId)
            .from(request.LocalDidToUseWithPrism)
            .to(new List<string>() { request.PrismDid })
            .build();

        var didComm = new DidComm(_didDocResolver, _secretResolver);
        var packResult = await didComm.PackEncrypted(
            new PackEncryptedParamsBuilder(mediateRequestMessage, to: request.PrismDid)
                .From(request.LocalDidToUseWithPrism)
                .ProtectSenderId(false)
                .BuildPackEncryptedParams()
        );


        if (packResult.IsFailed)
        {
            return packResult.ToResult();
        }

        HttpResponseMessage response = null;
        if (request.PrismEndpoint.Contains("did:peer:2"))
        {
            var resolvedEndpoint = PeerDID.PeerDIDCreateResolve.PeerDidResolver.ResolvePeerDid(new PeerDid(request.PrismEndpoint), VerificationMaterialFormatPeerDid.Jwk);
            if (resolvedEndpoint.IsFailed)
            {
                return Result.Fail($"Unable to resolve peer DID of the endpoint : {request.PrismEndpoint}");
            }

            var resolvedEndpointDidDoc = DidDocPeerDid.FromJson(resolvedEndpoint.Value);
            if (resolvedEndpointDidDoc.IsFailed)
            {
                return Result.Fail($"Unable to resolve peer DID of the endpoint : {request.PrismEndpoint}");
            }

            var isParsed = Uri.TryCreate(resolvedEndpointDidDoc.Value?.Services?.First().ServiceEndpoint, UriKind.Absolute, out var mediatorEndpointUri);
            if (!isParsed)
            {
                return Result.Fail($"Unable to resolve peer DID of the endpoint into URI: {resolvedEndpointDidDoc.Value?.Services?.First().ServiceEndpoint}");
            }

            // The other party sits behind a mediator

            try
            {
                var forwardMessageResult = await _mediator.Send(new SendForwardMessageRequest(
                    message: packResult.Value.PackedMessage,
                    localDid: request.LocalDidToUseWithPrism, // shouldn't that be a did just for the mediator or the other party?
                    mediatorDid: resolvedEndpointDidDoc.Value.Did,
                    mediatorEndpoint: mediatorEndpointUri!,
                    recipientDid: request.PrismDid
                ), new CancellationToken());
                if (forwardMessageResult.IsFailed)
                {
                    return Result.Fail($"Error sending a message to the contact: {forwardMessageResult.Errors.First().Message}");
                }

                // Since it is a forward message, we don't expect a direct-http response
            }
            catch (Exception e)
            {
                return Result.Fail(e.Message);
            }
        }
        else
        {
            try
            {
                response = await _httpClient.PostAsync(request.PrismEndpoint, new StringContent(packResult.Value.PackedMessage, new MediaTypeHeaderValue(MessageTyp.Encrypted)), cancellationToken);
            }
            catch (HttpRequestException ex)
            {
                return Result.Fail($"Connection could not be established: {ex.Message}");
            }

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return Result.Fail("Connection could not be established. Not found.");
            }

            if (!response.IsSuccessStatusCode)
            {
                return Result.Fail("Unable to initiate connection: " + response.StatusCode);
            }

            var content = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!string.IsNullOrEmpty(content))
            {
                // If a return message would have been supported:

                var unpackResult = await didComm.Unpack(
                    new UnpackParamsBuilder(content)
                        .SecretResolver(_secretResolver)
                        .BuildUnpackParams());
                if (unpackResult.IsFailed)
                {
                    return unpackResult.ToResult();
                }

                if (unpackResult.Value.Message.Type == ProtocolConstants.PrismConnectResponse)
                {
                    if (unpackResult.Value.Message.Body.ContainsKey("goal_code"))
                    {
                        var goalCodeJson = (JsonElement)unpackResult.Value.Message.Body!["goal_code"];
                        if (goalCodeJson.ValueKind == JsonValueKind.String && goalCodeJson.GetString()!.Equals(GoalCodes.PrismConnect, StringComparison.InvariantCultureIgnoreCase))
                        {
                            return Result.Ok(new PrismConnectResponse(unpackResult.Value.Message.Id, unpackResult.Value.Message.From ?? unpackResult.Value.Metadata!.EncryptedFrom));
                        }
                    }

                    return Result.Fail("The response does not have correct threadId, type or goalCode");
                }
                else
                {
                    return Result.Fail("Error: Unexpected message response type");
                }
            }
        }

        // The return message is hopefully at our mediator
        var retry = 0;
        var maxRetries = 15;
        do
        {
            await Task.Delay(1500, cancellationToken);
            var checkResult = await CheckMediatorForConnectResponse(request, cancellationToken);
            if (checkResult.IsSuccess)
            {
                var deleteResult = await DeleteConnectResponseFromMediator(request, checkResult.Value.MessageIdOfResponse!, cancellationToken);
                if (deleteResult.IsSuccess)
                {
                    return Result.Ok(new PrismConnectResponse(checkResult.Value.MessageIdOfResponse!, checkResult.Value.PrismDid!));
                }
            }
        } while (retry++ < maxRetries);

        return Result.Fail("Could not find a response from the PRISM agent in the mediator after multiple retries. Aborting");
    }

    private async Task<Result<PrismConnectResponse>> CheckMediatorForConnectResponse(PrismConnectRequest request, CancellationToken cancellationToken)
    {
        if (request.MediatorEndpoint is null || request.MediatorDid is null || request.LocalDidToUseWithMediator is null)
        {
            return Result.Fail("The answer to the connection-request cannot be fetched from the mediator, because the mediator-endpoint, mediator-did or local-did-to-use-with-mediator is not set.");
        }

        var deliveryResult = await _mediator.Send(new DeliveryRequestRequest(request.LocalDidToUseWithMediator, request.MediatorDid, request.MediatorEndpoint, 100, request.LocalDidToUseWithPrism), cancellationToken);
        if (deliveryResult.IsFailed)
        {
            return deliveryResult.ToResult();
        }

        if (deliveryResult.Value.ProblemReport is not null)
        {
            return Result.Ok(new PrismConnectResponse(deliveryResult.Value.ProblemReport));
        }

        if (deliveryResult.Value.HasMessages is false)
        {
            //TODO retry
            return Result.Fail("No messages found");
        }

        foreach (var message in deliveryResult.Value.Messages!)
        {
            if (message.Message!.Thid.Equals(request.ThreadId) && message.Message.Type.Equals(ProtocolConstants.PrismConnectResponse))
            {
                if (message.Message.Body.ContainsKey("goal_code"))
                {
                    var goalCodeJson = (JsonElement)message.Message.Body!["goal_code"];
                    if (goalCodeJson.ValueKind == JsonValueKind.String && goalCodeJson.GetString()!.Equals(GoalCodes.PrismConnect, StringComparison.InvariantCultureIgnoreCase))
                    {
                        return Result.Ok(new PrismConnectResponse(message.MessageId!, message.Message.From ?? message.Metadata!.EncryptedFrom));
                    }
                }
            }
        }

        return Result.Fail("No messages found, with correct threadId, type and goalCode");
    }

    private async Task<Result<ProblemReport>> DeleteConnectResponseFromMediator(PrismConnectRequest request, string messageId, CancellationToken cancellationToken)
    {
        if (request.LocalDidToUseWithMediator is null || request.MediatorDid is null || request.MediatorEndpoint is null)
        {
            return Result.Fail("The connection-response cannot be deleted from the  mediator, because the mediator-endpoint, mediator-did or local-did-to-use-with-mediator is not set.");
        }

        var messageReceivedResult = await _mediator.Send(new MessageReceivedRequest(request.LocalDidToUseWithMediator, request.MediatorDid, request.MediatorEndpoint, new List<string>() { messageId }), cancellationToken);
        if (messageReceivedResult.IsFailed)
        {
            return messageReceivedResult.ToResult();
        }

        if (messageReceivedResult.Value.ProblemReport is not null)
        {
            return Result.Ok(messageReceivedResult.Value.ProblemReport);
        }

        return Result.Ok();
    }
}