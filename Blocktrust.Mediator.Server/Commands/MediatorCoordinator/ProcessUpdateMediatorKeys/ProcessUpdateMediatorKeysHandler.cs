namespace Blocktrust.Mediator.Server.Commands.MediatorCoordinator.ProcessUpdateMediatorKeys;

using System.Text.Json;
using Blocktrust.DIDComm.Message.Messages;
using Blocktrust.Mediator.Common.Models.MediatorCoordinator;
using Blocktrust.Mediator.Common.Protocols;
using Common.Models.ProblemReport;
using DatabaseCommands.GetConnection;
using DatabaseCommands.UpdateRegisteredRecipients;
using FluentResults;
using MediatR;

public class ProcessUpdateMediatorKeysHandler : IRequestHandler<ProcessUpdateMediatorKeysRequest, Message>
{
    private readonly IMediator _mediator;

    /// <summary>
    /// Constructor
    /// </summary>
    public ProcessUpdateMediatorKeysHandler(IMediator mediator)
    {
        this._mediator = mediator;
    }

    /// <inheritdoc />
    public async Task<Message> Handle(ProcessUpdateMediatorKeysRequest request, CancellationToken cancellationToken)
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
            var body = request.UnpackedMessage.Body;
            var hasUpdates = body.TryGetValue("updates", out var updatesBody);
            if (!hasUpdates)
            {
                return ProblemReportMessage.BuildDefaultMessageMissingArguments(
                    errorMessage: "'updates' is missing in the body",
                    threadIdWhichCausedTheProblem: request.UnpackedMessage.Thid ?? request.UnpackedMessage.Id,
                    fromPrior: request.FromPrior);
            }

            var updatesJsonElement = (JsonElement)updatesBody!;
            var updatesJson = updatesJsonElement.GetRawText();
            try
            {
                var updates = JsonSerializer.Deserialize<List<KeyListUpdateModel>>(updatesJson);
                if (updates is null)
                {
                    return ProblemReportMessage.BuildDefaultMessageMissingArguments(
                        errorMessage: "Invalid body: unable to deserialize 'updates'",
                        threadIdWhichCausedTheProblem: request.UnpackedMessage.Thid ?? request.UnpackedMessage.Id,
                        fromPrior: request.FromPrior);
                }

                var addUpdates = updates.Where(p => p.UpdateType == "add").Select(p => p.KeyToUpdate).ToList();
                var removeUpdates = updates.Where(p => p.UpdateType == "remove").Select(p => p.KeyToUpdate).ToList();
                var updateResult = await _mediator.Send(new UpdateRegisteredRecipientsRequest(request.SenderDid, addUpdates, removeUpdates), cancellationToken);
                if (updateResult.IsFailed)
                {
                    return ProblemReportMessage.BuildDefaultInternalError(
                        errorMessage: "Unable to update key entries",
                        threadIdWhichCausedTheProblem: request.UnpackedMessage.Thid ?? request.UnpackedMessage.Id,
                        fromPrior: request.FromPrior);
                }
            }
            catch (Exception _)
            {
                return ProblemReportMessage.BuildDefaultMessageMissingArguments(
                    errorMessage: "Invalid body: unable to deserialize 'updates'",
                    threadIdWhichCausedTheProblem: request.UnpackedMessage.Thid ?? request.UnpackedMessage.Id,
                    fromPrior: request.FromPrior);
            }

            // Create the message to indicate a successful update
            var mediateGrantMessage = new MessageBuilder(
                    id: Guid.NewGuid().ToString(),
                    type: ProtocolConstants.CoordinateMediation2KeylistUpdateResponse,
                    body: new Dictionary<string, object>()
                    {
                        { "updated", updatesBody }
                    }
                )
                .thid(request.UnpackedMessage.Thid ?? request.UnpackedMessage.Id)
                .fromPrior(request.FromPrior)
                .build();
            return mediateGrantMessage;
        }
    }
}