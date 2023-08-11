namespace Blocktrust.Mediator.Server.Commands.Pickup.ProcessStatusRequest;

using System.Text.Json;
using Blocktrust.DIDComm.Message.Messages;
using Blocktrust.Mediator.Common.Protocols;
using Blocktrust.Mediator.Server.Commands.DatabaseCommands.GetMessagesStatus;
using Common.Models.ProblemReport;
using MediatR;

public class ProcessPickupStatusRequestHandler : IRequestHandler<ProcessPickupStatusRequestRequest, Message>
{
    private readonly IMediator _mediator;

    /// <summary>
    /// Constructor
    /// </summary>
    public ProcessPickupStatusRequestHandler(IMediator mediator)
    {
        this._mediator = mediator;
    }

    /// <inheritdoc />
    public async Task<Message> Handle(ProcessPickupStatusRequestRequest request, CancellationToken cancellationToken)
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

        var getStatusResult = await _mediator.Send(new GetMessagesStatusRequest(request.SenderDid, request.MediatorDid, recipientDid), cancellationToken);
        if (getStatusResult.IsFailed)
        {
            return ProblemReportMessage.BuildDefaultMessageMissingArguments(
                errorMessage:getStatusResult.Errors.FirstOrDefault() is null ? "Unknown error" : getStatusResult.Errors.First().Message,
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