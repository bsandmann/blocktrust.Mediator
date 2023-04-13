namespace Blocktrust.Mediator.Server.Commands.ForwardMessage;

using System.Text.Json;
using Blocktrust.DIDComm.Message.Attachments;
using Blocktrust.DIDComm.Utils;
using Blocktrust.Mediator.Server.Commands.DatabaseCommands.StoreMessage;
using Blocktrust.Mediator.Server.Models;
using Common.Models.ProblemReport;
using DIDComm.Message.Messages;
using MediatR;

public class ProcessForwardMessageHandler : IRequestHandler<ProcessForwardMessageRequest, Message?>
{
    private readonly IMediator _mediator;

    /// <summary>
    /// Constructor
    /// </summary>
    public ProcessForwardMessageHandler(IMediator mediator)
    {
        this._mediator = mediator;
    }

    // TODO dig into https://identity.foundation/didcomm-messaging/spec/#routing-protocol-20

    /// <inheritdoc />
    public async Task<Message?> Handle(ProcessForwardMessageRequest request, CancellationToken cancellationToken)
    {
        if (request.MediatorDid is null)
        {
            return ProblemReportMessage.BuildDefaultInternalError(
                errorMessage:  "Invalid body: mediator did is empty",
                threadIdWhichCausedTheProblem: request.UnpackedMessage.Thid ?? request.UnpackedMessage.Id,
                fromPrior: request.FromPrior); 
        }
        
        var body = request.UnpackedMessage.Body;
        var hasNext = body.TryGetValue("next", out var next);
        if (!hasNext)
        {
            return ProblemReportMessage.BuildDefaultMessageMissingArguments(
                errorMessage: "Invalid body",
                threadIdWhichCausedTheProblem: request.UnpackedMessage.Thid ?? request.UnpackedMessage.Id,
                fromPrior: request.FromPrior);
        }

        var nextJsonElement = (JsonElement)next!;
        var recipientDid = nextJsonElement.GetString();

        //TODO check if recipient is a valid DID (a single DID, not multiple DIDs)?
        if (string.IsNullOrEmpty(recipientDid))
        {
            return ProblemReportMessage.BuildDefaultMessageMissingArguments(
                errorMessage: "Invalid body: recipient did is empty",
                threadIdWhichCausedTheProblem: request.UnpackedMessage.Thid ?? request.UnpackedMessage.Id,
                fromPrior: request.FromPrior);
        }

        // TODO Possible code duplication with the DeliveryRequestHandler
        var attachments = request.UnpackedMessage.Attachments;
        var innerMessages = new List<StoredMessageModel>();
        foreach (var attachment in attachments!)
        {
            var id = attachment.Id ?? Guid.NewGuid().ToString();
            var data = attachment.Data;
            if (data is Json)
            {
                Json? jsonAttachmentData = (Json)data;
                var msg = jsonAttachmentData?.JsonString;
                var innerMessage = JsonSerializer.Serialize(msg, SerializationOptions.UnsafeRelaxedEscaping);
                innerMessages.Add(new StoredMessageModel(id, innerMessage));
            }
            else
            {
                return ProblemReportMessage.BuildDefaultInternalError(
                    errorMessage: "Not implemented yet. Use JSON format",
                    threadIdWhichCausedTheProblem: request.UnpackedMessage.Thid ?? request.UnpackedMessage.Id,
                    fromPrior: request.FromPrior);

                throw new NotImplementedException("Not implemented yet");
            }
        }

        var storeMessageResult = await _mediator.Send(new StoreMessagesRequest(request.MediatorDid, recipientDid, innerMessages), cancellationToken);
        if (storeMessageResult.IsFailed)
        {
            return ProblemReportMessage.BuildDefaultInternalError(
                errorMessage: storeMessageResult.Errors.FirstOrDefault().Message,
                threadIdWhichCausedTheProblem: request.UnpackedMessage.Thid ?? request.UnpackedMessage.Id,
                fromPrior: request.FromPrior);
        }

        return null;
        // }
    }
}