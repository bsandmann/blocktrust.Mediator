namespace Blocktrust.Mediator.Client.Commands.PrismConnect.AnwserConnectRequest;

using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Nodes;
using Blocktrust.Common.Resolver;
using Common;
using Common.Models.ProblemReport;
using Common.Protocols;
using DIDComm;
using DIDComm.Common.Types;
using DIDComm.Message.Messages;
using DIDComm.Model.PackEncryptedParamsModels;
using FluentResults;
using MediatR;
using Pickup.DeliveryRequest;
using Pickup.MessageReceived;
using ProcessOobInvitationAndConnect;

public class AnswerPrismConnectHandler : IRequestHandler<AnswerPrismConnectRequest, Result<AnswerPrismConnectResponse>>
{
    private readonly HttpClient _httpClient;
    private readonly IDidDocResolver _didDocResolver;
    private readonly ISecretResolver _secretResolver;
    private readonly IMediator _mediator;

    public AnswerPrismConnectHandler(HttpClient httpClient, IDidDocResolver didDocResolver, ISecretResolver secretResolver, IMediator mediator)
    {
        _httpClient = httpClient;
        _didDocResolver = didDocResolver;
        _secretResolver = secretResolver;
        _mediator = mediator;
    }


    public async Task<Result<AnswerPrismConnectResponse>> Handle(AnswerPrismConnectRequest request, CancellationToken cancellationToken)
    {
        var retry = 0;
        var maxRetries = request.MaxTimeToWait.TotalSeconds < 1.5 ? 1.5 : Math.Round(request.MaxTimeToWait.TotalSeconds / 1.5);
        Result<AnswerPrismConnectResponse> checkResult;
        do
        {
            await Task.Delay(1500, cancellationToken);
            checkResult = await CheckMediatorForConnectResponse(request, cancellationToken);
            if (checkResult.IsSuccess)
            {
                var deleteResult = await DeleteConnectResponseFromMediator(request, checkResult.Value.MessageIdOfResponse!, cancellationToken);
                if (deleteResult.IsFailed)
                {
                    return deleteResult.ToResult();
                }
                break;
            }
        } while (retry++ < maxRetries);

        if (checkResult.IsFailed)
        {
            return checkResult;
        }

        // The special format of the return_route header is required by the python implementation of the roots mediator
        var returnRoute = new JsonObject() { new KeyValuePair<string, JsonNode?>("return_route", "all") };
        var body = new Dictionary<string, object>();
        body.Add("goal_code", GoalCodes.PrismConnect);
        body.Add("goal", "Connect");
        var acceptList = new List<string>() { "didcomm/v2" };
        body.Add("accept", acceptList);
        var mediateRequestMessage = new MessageBuilder(
                id: Guid.NewGuid().ToString(),
                type: ProtocolConstants.PrismConnectResponse,
                body: body
            )
            .customHeader("custom_headers", new List<JsonObject>() { returnRoute })
            .customHeader("return_route", "all")
            .thid(request.MessageId)
            .from(request.LocalPeerDidUsedInOobInvitation.Value)
            .to(new List<string>() { checkResult.Value.PrismDid! })
            .build();

        var didComm = new DidComm(_didDocResolver, _secretResolver);
        var packResult = await didComm.PackEncrypted(
            new PackEncryptedParamsBuilder(mediateRequestMessage, to: checkResult.Value.PrismDid!)
                .From(request.LocalPeerDidUsedInOobInvitation.Value)
                .ProtectSenderId(false)
                .BuildPackEncryptedParams()
        );

        var prismDidDoc = await _didDocResolver.Resolve(checkResult.Value.PrismDid!);
        if (prismDidDoc is null)
        {
            return Result.Fail("Error resolving prism did");
        }

        var prismAgentEndpoint = prismDidDoc.Services.FirstOrDefault().ServiceEndpoint;
        if (string.IsNullOrEmpty(prismAgentEndpoint))
        {
            return Result.Fail("Error parsing the endpoint of the prism did");
        }

#if DEBUG
        // The problem is that the prism agent is running in a docker container and the mediator is running on the host machine.
        // For more details read the documentation in the test for this method
        prismAgentEndpoint = prismAgentEndpoint.Replace("host.docker.internal", "localhost");
#endif

        var prismDidEndpointUri = new Uri(prismAgentEndpoint);

        // We send the message to agent
        HttpResponseMessage response;
        try
        {
            response = await _httpClient.PostAsync(prismDidEndpointUri, new StringContent(packResult.PackedMessage, new MediaTypeHeaderValue(MessageTyp.Encrypted)), cancellationToken);
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

        return new AnswerPrismConnectResponse(checkResult.Value.MessageIdOfResponse!, checkResult.Value.PrismDid!);
    }

    private async Task<Result<AnswerPrismConnectResponse>> CheckMediatorForConnectResponse(AnswerPrismConnectRequest request, CancellationToken cancellationToken)
    {
        var deliveryResult = await _mediator.Send(new DeliveryRequestRequest(request.LocalDidToUseWithMediator, request.MediatorDid, request.MediatorEndpoint, 100, request.LocalPeerDidUsedInOobInvitation.Value), cancellationToken);
        if (deliveryResult.IsFailed)
        {
            return deliveryResult.ToResult();
        }

        if (deliveryResult.Value.ProblemReport is not null)
        {
            return Result.Ok(new AnswerPrismConnectResponse(deliveryResult.Value.ProblemReport));
        }

        if (deliveryResult.Value.HasMessages is false)
        {
            return Result.Fail("No messages found");
        }

        foreach (var message in deliveryResult.Value.Messages!)
        {
            if (message.Message!.Thid.Equals(request.MessageId) && message.Message.Type.Equals(ProtocolConstants.PrismConnectRequest))
            {
                if (message.Message.Body.ContainsKey("goal_code"))
                {
                    var goalCodeJson = (JsonElement)message.Message.Body!["goal_code"];
                    if (goalCodeJson.ValueKind == JsonValueKind.String && goalCodeJson.GetString()!.Equals(GoalCodes.PrismConnectOob, StringComparison.InvariantCultureIgnoreCase))
                    {
                        return Result.Ok(new AnswerPrismConnectResponse(message.MessageId!, message.Message.From ?? message.Metadata!.EncryptedFrom));
                    }
                }
            }
        }

        return Result.Fail("No messages found, with correct threadId, type and goalCode");
    }
    
     private async Task<Result<ProblemReport>> DeleteConnectResponseFromMediator(AnswerPrismConnectRequest request, string messageId, CancellationToken cancellationToken)
        {
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