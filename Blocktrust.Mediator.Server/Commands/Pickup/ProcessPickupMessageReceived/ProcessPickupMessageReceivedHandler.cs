namespace Blocktrust.Mediator.Server.Commands.Pickup.ProcessPickupMessageReceived;

using System.Text.Json;
using Blocktrust.DIDComm.Message.Messages;
using Blocktrust.Mediator.Common.Models.ProblemReport;
using Blocktrust.Mediator.Common.Protocols;
using Blocktrust.Mediator.Server.Commands.DatabaseCommands.DeleteMessages;
using Blocktrust.Mediator.Server.Commands.DatabaseCommands.GetMessagesStatus;
using MediatR;

public class ProcessPickupMessageReceivedHandler : IRequestHandler<ProcessPickupMessageReceivedRequest, Message>
{
    private readonly IMediator _mediator;

    /// <summary>
    /// Constructor
    /// </summary>
    public ProcessPickupMessageReceivedHandler(IMediator mediator)
    {
        this._mediator = mediator;
    }

    /// <inheritdoc />
    public async Task<Message> Handle(ProcessPickupMessageReceivedRequest request, CancellationToken cancellationToken)
    {
        var body = request.UnpackedMessage.Body;
        var hasMessageIdList = body.TryGetValue("message_id_list", out var messageIdListJson);
        if (!hasMessageIdList)
        {
            return ProblemReportMessage.BuildDefaultMessageMissingArguments(
                errorMessage: "Invalid body format: missing 'message_id_list'",
                threadIdWhichCausedTheProblem: request.UnpackedMessage.Thid ?? request.UnpackedMessage.Id,
                fromPrior: request.FromPrior);
        }

        var messageIdListJsonElement = (JsonElement)messageIdListJson!;
        var messageIdList = new List<string>();
        if (messageIdListJsonElement.ValueKind is JsonValueKind.Array)
        {
            foreach (var idJsonElement in messageIdListJsonElement.EnumerateArray())
            {
                if (idJsonElement.ValueKind is not JsonValueKind.String)
                {
                    return ProblemReportMessage.BuildDefaultMessageMissingArguments(
                        errorMessage: "Invalid body format: incorrect entries in 'message_id_list'",
                        threadIdWhichCausedTheProblem: request.UnpackedMessage.Thid ?? request.UnpackedMessage.Id,
                        fromPrior: request.FromPrior);
                }
                messageIdList.Add(idJsonElement!.GetString());
            }
        }
        else
        {
            return ProblemReportMessage.BuildDefaultMessageMissingArguments(
                errorMessage: "Invalid body format: 'message_id_list'",
                threadIdWhichCausedTheProblem: request.UnpackedMessage.Thid ?? request.UnpackedMessage.Id,
                fromPrior: request.FromPrior);
        }
        
        if (request.SenderDid is null || request.MediatorDid is null)
        {
            return ProblemReportMessage.BuildDefaultMessageMissingArguments(
                errorMessage: "Invalid body format: missing sender_did or mediator_did",
                threadIdWhichCausedTheProblem: request.UnpackedMessage.Thid ?? request.UnpackedMessage.Id,
                fromPrior: request.FromPrior);
        }

        var deleteMessagesResult = await _mediator.Send(new DeleteMessagesRequest(request.SenderDid, request.MediatorDid, messageIdList), cancellationToken);
        if (deleteMessagesResult.IsFailed)
        {
            return ProblemReportMessage.BuildDefaultInternalError(
                errorMessage: deleteMessagesResult.Errors.FirstOrDefault().Message,
                threadIdWhichCausedTheProblem: request.UnpackedMessage.Thid ?? request.UnpackedMessage.Id,
                fromPrior: request.FromPrior);
        }
        
        var getStatusResult = await _mediator.Send(new GetMessagesStatusRequest(request.SenderDid, request.MediatorDid, null), cancellationToken);
        if (getStatusResult.IsFailed)
        {
            return ProblemReportMessage.BuildDefaultInternalError(
                errorMessage: getStatusResult.Errors.FirstOrDefault().Message,
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