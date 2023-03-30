namespace Blocktrust.Mediator.Server.Commands.Pickup.ProcessPickupDeliveryRequest;

using System.Text.Json;
using Blocktrust.DIDComm.Message.Attachments;
using Blocktrust.DIDComm.Message.Messages;
using Blocktrust.Mediator.Common.Protocols;
using Blocktrust.Mediator.Server.Commands.DatabaseCommands.GetMessages;
using Blocktrust.Mediator.Server.Commands.DatabaseCommands.GetMessagesStatus;
using Common.Models.ProblemReport;
using MediatR;

public class ProcessPickupDeliveryRequestHandler : IRequestHandler<ProcessPickupDeliveryRequestRequest, Message>
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
    public async Task<Message> Handle(ProcessPickupDeliveryRequestRequest request, CancellationToken cancellationToken)
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
                return ProblemReportMessage.BuildDefaultMessageMissingArguments(
                    errorMessage: "Invalid body format: recipient_did",
                    threadIdWhichCausedTheProblem: request.UnpackedMessage.Thid ?? request.UnpackedMessage.Id,
                    fromPrior: request.FromPrior);
            }
        }
        
        if (request.SenderDid is null || request.MediatorDid is null)
        {
            return ProblemReportMessage.BuildDefaultMessageMissingArguments(
                errorMessage: "Invalid body format: missing sender_did or mediator_did",
                threadIdWhichCausedTheProblem: request.UnpackedMessage.Thid ?? request.UnpackedMessage.Id,
                fromPrior: request.FromPrior);
        }

        var getMessagesResult = await _mediator.Send(new GetMessagesRequest(request.SenderDid, request.MediatorDid, recipientDid), cancellationToken);
        if (getMessagesResult.IsFailed)
        {
            return ProblemReportMessage.BuildDefaultMessageMissingArguments(
                errorMessage: getMessagesResult.Errors.FirstOrDefault().Message,
                threadIdWhichCausedTheProblem: request.UnpackedMessage.Thid ?? request.UnpackedMessage.Id,
                fromPrior: request.FromPrior);
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
                    return ProblemReportMessage.BuildDefaultInternalError(
                        errorMessage: "Internal error deserializing",
                        threadIdWhichCausedTheProblem: request.UnpackedMessage.Thid ?? request.UnpackedMessage.Id,
                        fromPrior: request.FromPrior);
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
                .thid(request.UnpackedMessage.Thid ?? request.UnpackedMessage.Id)
                .fromPrior(request.FromPrior)
                .attachments(attachments)
                .build();

            return deliveryResponseMessage;
        }

        // If we don't have any messages we return a status-message instead of a empty list

        var getStatusResult = await _mediator.Send(new GetMessagesStatusRequest(request.SenderDid, request.MediatorDid, recipientDid), cancellationToken);
        if (getStatusResult.IsFailed)
        {
            return ProblemReportMessage.BuildDefaultInternalError(
                errorMessage: getMessagesResult.Errors.FirstOrDefault().Message,
                threadIdWhichCausedTheProblem: request.UnpackedMessage.Thid ?? request.UnpackedMessage.Id,
                fromPrior: request.FromPrior);
        }

        var statusMessage = new MessageBuilder(
                id: Guid.NewGuid().ToString(),
                type: ProtocolConstants.MessagePickup3StatusResponse,
                body: getStatusResult.Value.GetMessagePickup3StatusResponseBody()
            )
            .thid(request.UnpackedMessage.Thid ?? request.UnpackedMessage.Id)
            .fromPrior(request.FromPrior)
            .build();
        return statusMessage;
    }
}