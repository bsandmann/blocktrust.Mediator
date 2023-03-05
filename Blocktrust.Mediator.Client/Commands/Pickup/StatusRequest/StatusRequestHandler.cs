namespace Blocktrust.Mediator.Client.Commands.Pickup.StatusRequest;

using System.Net;
using System.Text;
using System.Text.Json;
using Blocktrust.Common.Resolver;
using Common.Models.Pickup;
using Common.Protocols;
using DIDComm;
using DIDComm.Common.Types;
using DIDComm.Message.Attachments;
using DIDComm.Message.Messages;
using DIDComm.Model.PackEncryptedParamsModels;
using DIDComm.Model.UnpackParamsModels;
using FluentResults;
using ForwardMessage;
using MediatR;

public class StatusRequestHandler : IRequestHandler<StatusRequestRequest, Result<StatusRequestResponse>>
{
    private readonly IMediator _mediator;
    private readonly HttpClient _httpClient;
    private readonly IDidDocResolver _didDocResolver;
    private readonly ISecretResolver _secretResolver;

    public StatusRequestHandler(IMediator mediator, HttpClient httpClient, IDidDocResolver didDocResolver, ISecretResolver secretResolver)
    {
        _mediator = mediator;
        _httpClient = httpClient;
        _didDocResolver = didDocResolver;
        _secretResolver = secretResolver;
    }

    public async Task<Result<StatusRequestResponse>> Handle(StatusRequestRequest request, CancellationToken cancellationToken)
    {
        var body = new Dictionary<string, object>();
        if (!string.IsNullOrEmpty(request.RecipientDid))
        {
            body.Add("recipient_did", request.RecipientDid);
        }

        var statusRequestMessage = new MessageBuilder(
                id: Guid.NewGuid().ToString(),
                type: ProtocolConstants.MessagePickup3StatusRequest,
                body: body
            )
            .to(new List<string>() { request.MediatorDid })
            .build();

        var didComm = new DidComm(_didDocResolver, _secretResolver);

        // We pack the message and encrypt it for the mediator
        var packResult = didComm.PackEncrypted(
            new PackEncryptedParamsBuilder(statusRequestMessage, to: request.MediatorDid)
                .From(request.LocalDid)
                .ProtectSenderId(false)
                .BuildPackEncryptedParams()
        );

        // We send the message to the mediator endpoint
        var response = await _httpClient.PostAsync(request.MediatorEndpoint, new StringContent(packResult.PackedMessage, Encoding.UTF8, MessageTyp.Encrypted), cancellationToken);

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

        if (unpackResult.Value.Message.Type != ProtocolConstants.MessagePickup3StatusResponse)
        {
            return Result.Fail($"Unexpected header-type: {unpackResult.Value.Message.Type}");
        }

        var bodyContent = unpackResult.Value.Message.Body;
        var statusRequestResponse = new StatusRequestResponse();
        if (bodyContent.ContainsKey("message_count"))
        {
            bodyContent.TryGetValue("message_count", out var messageCount);
            var messageCountJsonElement = (JsonElement)messageCount;
            if (messageCountJsonElement.ValueKind is JsonValueKind.Number)
            {
                statusRequestResponse.MessageCount = messageCountJsonElement.GetInt32();
            }
        }
        else
        {
            return Result.Fail("Required content: message_count is missing in the body");
        }

        if (bodyContent.ContainsKey("recipient_did"))
        {
            bodyContent.TryGetValue("recipient_did", out var recipientDid);
            var recipientDidJsonElement = (JsonElement)recipientDid;
            if (recipientDidJsonElement.ValueKind is JsonValueKind.String)
            {
                statusRequestResponse.RecipientDid = recipientDidJsonElement.GetString();
            }
        }

        if (bodyContent.ContainsKey("longest_waited_seconds"))
        {
            bodyContent.TryGetValue("longest_waited_seconds", out var longestWaitedSeconds);
            var longestWaitedSecondsJsonElement = (JsonElement)longestWaitedSeconds;
            if (longestWaitedSecondsJsonElement.ValueKind is JsonValueKind.Number)
            {
                statusRequestResponse.LongestWaitedSeconds = longestWaitedSecondsJsonElement.GetInt64();
            }
        }

        if (bodyContent.ContainsKey("newest_received_time"))
        {
            bodyContent.TryGetValue("newest_received_time", out var newestMessageTime);
            var newestMessageTimeJsonElement = (JsonElement)newestMessageTime;
            if (newestMessageTimeJsonElement.ValueKind is JsonValueKind.Number)
            {
                statusRequestResponse.LongestWaitedSeconds = newestMessageTimeJsonElement.GetInt64();
            }
        }

        if (bodyContent.ContainsKey("oldest_received_time"))
        {
            bodyContent.TryGetValue("oldest_received_time", out var oldestMessageTime);
            var oldestMessageTimeJsonElement = (JsonElement)oldestMessageTime;
            if (oldestMessageTimeJsonElement.ValueKind is JsonValueKind.Number)
            {
                statusRequestResponse.LongestWaitedSeconds = oldestMessageTimeJsonElement.GetInt64();
            }
        }

        if (bodyContent.ContainsKey("total_bytes"))
        {
            bodyContent.TryGetValue("total_bytes", out var totalByteSize);
            var totalByteSizeJsonElement = (JsonElement)totalByteSize;
            if (totalByteSizeJsonElement.ValueKind is JsonValueKind.Number)
            {
                statusRequestResponse.LongestWaitedSeconds = totalByteSizeJsonElement.GetInt64();
            }
        }

        if (bodyContent.ContainsKey("live_delivery"))
        {
            bodyContent.TryGetValue("live_delivery", out var liveDelivery);
            var liveDeliveryJsonElement = (JsonElement)liveDelivery;
            if (liveDeliveryJsonElement.ValueKind is JsonValueKind.True || liveDeliveryJsonElement.ValueKind is JsonValueKind.False)
            {
                statusRequestResponse.LiveDelivery = liveDeliveryJsonElement.GetBoolean();
            }
        }

        return Result.Ok(statusRequestResponse);
    }
}