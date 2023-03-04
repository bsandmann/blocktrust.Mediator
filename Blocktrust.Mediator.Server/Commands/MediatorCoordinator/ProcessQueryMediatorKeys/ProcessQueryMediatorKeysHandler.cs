namespace Blocktrust.Mediator.Server.Commands.MediatorCoordinator.AnswerMediation;

using System.Text.Json;
using System.Text.Json.Nodes;
using Blocktrust.Common.Resolver;
using Common.Commands.CreatePeerDid;
using Common.Models.MediatorCoordinator;
using Common.Protocols;
using Connections.CreateConnection;
using Connections.GetConnection;
using Connections.GetKeyEntries;
using Connections.UpdateConnection;
using Connections.UpdateKeyEntries;
using DIDComm.Message.Messages;
using Entities;
using FluentResults;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ProcessQueryMediatorKeys;
using ProcessUpdateMediatorKeys;

public class ProcessQueryMediatorKeysHandler : IRequestHandler<ProcessQueryMediatorKeysRequest, Result<Message>>
{
    private readonly DataContext _context;
    private readonly IMediator _mediator;
    private readonly ISecretResolver _secretResolver;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="context"></param>
    public ProcessQueryMediatorKeysHandler(DataContext context, IMediator mediator, ISecretResolver secretResolver)
    {
        this._context = context;
        this._mediator = mediator;
        this._secretResolver = secretResolver;
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
            var queryResult = await _mediator.Send(new GetKeyEntriesRequest(request.SenderDid), cancellationToken);
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