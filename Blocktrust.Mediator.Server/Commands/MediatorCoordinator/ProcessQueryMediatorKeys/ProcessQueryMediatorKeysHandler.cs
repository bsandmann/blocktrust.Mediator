namespace Blocktrust.Mediator.Server.Commands.MediatorCoordinator.ProcessQueryMediatorKeys;

using Blocktrust.DIDComm.Message.Messages;
using Blocktrust.Mediator.Common.Models.MediatorCoordinator;
using Blocktrust.Mediator.Common.Protocols;
using DatabaseCommands.GetConnection;
using DatabaseCommands.GetRegisteredRecipients;
using FluentResults;
using MediatR;

public class ProcessQueryMediatorKeysHandler : IRequestHandler<ProcessQueryMediatorKeysRequest, Result<Message>>
{
    private readonly IMediator _mediator;

    /// <summary>
    /// Constructor
    /// </summary>
    public ProcessQueryMediatorKeysHandler(IMediator mediator)
    {
        this._mediator = mediator;
    }

    public async Task<Result<Message>> Handle(ProcessQueryMediatorKeysRequest request, CancellationToken cancellationToken)
    {
        var existingConnection = await _mediator.Send(new GetConnectionRequest(request.SenderDid), cancellationToken);
        if (existingConnection.IsFailed)
        {
            // database error
        }

        if (existingConnection.Value is null && !existingConnection.Value!.MediationGranted)
        {
            return Result.Fail("Connection does not exist or mediation is not granted");
        }
        else
        {
            var queryResult = await _mediator.Send(new GetRegisteredRecipientsRequest(request.SenderDid), cancellationToken);
            if (queryResult.IsFailed)
            {
                return Result.Fail("Unable to read keys from database");
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
            return Result.Ok(mediateGrantMessage);
        }
    }
}