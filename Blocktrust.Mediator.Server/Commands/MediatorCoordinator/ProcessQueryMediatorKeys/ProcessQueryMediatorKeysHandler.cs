namespace Blocktrust.Mediator.Server.Commands.MediatorCoordinator.ProcessQueryMediatorKeys;

using Blocktrust.DIDComm.Message.Messages;
using Blocktrust.Mediator.Common.Models.MediatorCoordinator;
using Blocktrust.Mediator.Common.Protocols;
using Common.Models.ProblemReport;
using DatabaseCommands.GetConnection;
using DatabaseCommands.GetRegisteredRecipients;
using FluentResults;
using MediatR;

public class ProcessQueryMediatorKeysHandler : IRequestHandler<ProcessQueryMediatorKeysRequest, Message>
{
    private readonly IMediator _mediator;

    /// <summary>
    /// Constructor
    /// </summary>
    public ProcessQueryMediatorKeysHandler(IMediator mediator)
    {
        this._mediator = mediator;
    }

    /// <inheritdoc />
    public async Task<Message> Handle(ProcessQueryMediatorKeysRequest request, CancellationToken cancellationToken)
    {
        var existingConnection = await _mediator.Send(new GetConnectionRequest(request.SenderDid, request.MediatorDid), cancellationToken);
        if (existingConnection.IsFailed)
        {
            return ProblemReportMessage.BuildDefaultInternalError(
                errorMessage: "Unknown database error",
                threadIdWhichCausedTheProblem: request.UnpackedMessage.Thid ?? request.UnpackedMessage.Id,
                fromPrior: request.FromPrior);
        }

        if (existingConnection.Value is null && !existingConnection.Value!.MediationGranted)
        {
            return ProblemReportMessage.Build(
                fromPrior: request.FromPrior,
                threadIdWhichCausedTheProblem: request.UnpackedMessage.Thid ?? request.UnpackedMessage.Id,
                problemCode: new ProblemCode(
                    sorter: EnumProblemCodeSorter.Error,
                    scope: EnumProblemCodeScope.Message,
                    stateNameForScope: null,
                    descriptor: EnumProblemCodeDescriptor.Message,
                    otherDescriptor: null
                ),
                comment: $"Connection does not exist or mediation is not granted",
                commentArguments: null,
                escalateTo: new Uri("mailto:info@blocktrust.dev"));
        }
        else
        {
            var queryResult = await _mediator.Send(new GetRegisteredRecipientsRequest(request.SenderDid), cancellationToken);
            if (queryResult.IsFailed)
            {
                return ProblemReportMessage.BuildDefaultInternalError(
                    errorMessage: "Unable to read recipientDid from database",
                    threadIdWhichCausedTheProblem:
                    request.UnpackedMessage.Thid ?? request.UnpackedMessage.Id,
                    fromPrior: request.FromPrior);
            }

            // Create the message to indicate a successful update
            var mediateGrantMessage = new MessageBuilder(
                    id: Guid.NewGuid().ToString(),
                    type: ProtocolConstants.CoordinateMediation2KeylistQueryResponse,
                    body: new Dictionary<string, object>()
                    {
                        { "keys", queryResult.Value.Select(p => new KeyListModel() { QueryResult = p.KeyEntry }).ToList() }
                    }
                )
                .fromPrior(request.FromPrior)
                .build();
            return mediateGrantMessage;
        }
    }
}