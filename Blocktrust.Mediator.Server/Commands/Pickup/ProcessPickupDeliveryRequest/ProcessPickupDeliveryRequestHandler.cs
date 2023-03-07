namespace Blocktrust.Mediator.Server.Commands.Pickup.ProcessPickupDeliveryRequest;

using System.Text.Json;
using Blocktrust.DIDComm.Message.Attachments;
using Blocktrust.DIDComm.Message.Messages;
using Blocktrust.Mediator.Common.Protocols;
using Blocktrust.Mediator.Server.Commands.DatabaseCommands.GetMessages;
using Blocktrust.Mediator.Server.Commands.DatabaseCommands.GetMessagesStatus;
using FluentResults;
using MediatR;

public class ProcessPickupDeliveryRequestHandler : IRequestHandler<ProcessPickupDeliveryRequestRequest, Result<Message>>
{
    private readonly IMediator _mediator;

    /// <summary>
    /// Constructor
    /// </summary>
    public ProcessPickupDeliveryRequestHandler(IMediator mediator)
    {
        this._mediator = mediator;
    }

    /// <inheritdoc />
    public async Task<Result<Message>> Handle(ProcessPickupDeliveryRequestRequest request, CancellationToken cancellationToken)
    {
        var body = request.UnpackedMessage.Body;
        var hasRecipientDid = body.TryGetValue("recipient_did", out var recipientDidBody);
        string? recipientDid = null;
        if (hasRecipientDid)
        {
            var recipientDidJsonElement = (JsonElement)recipientDidBody!;
            if (recipientDidJsonElement.ValueKind is JsonValueKind.String)
            {
                //TODO check for valid did
                recipientDid = recipientDidJsonElement.GetString();
            }
            else
            {
                return Result.Fail("Invalid body format: recipient_did");
            }
        }

        var getMessagesResult = await _mediator.Send(new GetMessagesRequest(request.SenderDid, request.MediatorDid, recipientDid), cancellationToken);
        if (getMessagesResult.IsFailed)
        {
            return getMessagesResult.ToResult();
        }

        if (getMessagesResult.Value.Count != 0)
        {
            var returnBodyMessageList = new Dictionary<string, object>();
            if (recipientDid is not null)
            {
                returnBodyMessageList.Add("recipient_did", recipientDid);
            }

            var attachments = new List<Attachment>();
            foreach (var storedMessageModel in getMessagesResult.Value)
            {
                var packedMessage = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(storedMessageModel.Message);
                if (packedMessage is null)
                {
                    // TODO
                    // Should never happen
                }
                attachments.Add(
                    new AttachmentBuilder(
                        id: storedMessageModel.MessageId,
                        data: new Json(json: packedMessage!)
                    ).Build());
            }

            var deliveryResponseMessage = new MessageBuilder(
                    id: Guid.NewGuid().ToString(),
                    type: ProtocolConstants.MessagePickup3DeliveryResponse,
                    body: returnBodyMessageList
                )
                .fromPrior(request.FromPrior)
                .attachments(attachments)
                .build();
            
            return Result.Ok(deliveryResponseMessage);
        }

        // If we don't have any messages we return a status-message instead of a empty list

        var getStatusResult = await _mediator.Send(new GetMessagesStatusRequest(request.SenderDid, request.MediatorDid, recipientDid), cancellationToken);
        if (getStatusResult.IsFailed)
        {
            return getStatusResult.ToResult();
        }

        var statusMessage = new MessageBuilder(
                id: Guid.NewGuid().ToString(),
                type: ProtocolConstants.MessagePickup3StatusResponse,
                body: getStatusResult.Value.GetMessagePickup3StatusResponseBody()
            )
            .fromPrior(request.FromPrior)
            .build();
        return Result.Ok(statusMessage);
    }
}