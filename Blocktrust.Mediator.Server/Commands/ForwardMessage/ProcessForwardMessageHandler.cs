namespace Blocktrust.Mediator.Server.Commands.ForwardMessage;

using System.Text.Json;
using Blocktrust.DIDComm.Message.Attachments;
using Blocktrust.DIDComm.Utils;
using Blocktrust.Mediator.Server.Commands.DatabaseCommands.GetConnection;
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
        var existingConnection = await _mediator.Send(new GetConnectionRequest(request.SenderDid, request.MediatorDid), cancellationToken);
        if (existingConnection.IsFailed)
        {
            // database error
        }

        if (existingConnection.Value is null && !existingConnection.Value!.MediationGranted)
        {
            return ProblemReportMessage.BuildDefaultMessageMissingArguments(
                errorMessage: "Connection does not exist or mediation is not granted",
                threadIdWhichCausedTheProblem: request.UnpackedMessage.Thid ?? request.UnpackedMessage.Id,
                fromPrior: request.FromPrior);
        }
        else
        {
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
            string innerMessage;
            var messages = new List<StoredMessageModel>();
            foreach (var attachment in attachments!)
            {
                var id = attachment.Id;
                var data = attachment.Data;
                if (data is Json)
                {
                    Json? jsonAttachmentData = (Json)data;
                    var innerJson = jsonAttachmentData?.JsonString;
                    var msg = innerJson?.GetTyped<Dictionary<string, object>>("json");
                    innerMessage = JsonSerializer.Serialize(msg);
                    messages.Add(new StoredMessageModel(id, innerMessage));
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

            var storeMessageResult = await _mediator.Send(new StoreMessagesRequest(request.MediatorDid, recipientDid, messages), cancellationToken);
            if (storeMessageResult.IsFailed)
            {
                return ProblemReportMessage.BuildDefaultInternalError(
                    errorMessage: storeMessageResult.Errors.FirstOrDefault().Message,
                    threadIdWhichCausedTheProblem: request.UnpackedMessage.Thid ?? request.UnpackedMessage.Id,
                    fromPrior: request.FromPrior);
            }

            return null;
        }
    }
}