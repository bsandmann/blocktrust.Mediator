namespace Blocktrust.Mediator.Server.Commands.MediatorCoordinator.AnswerMediation;

using System.Text.Json;
using System.Text.Json.Nodes;
using Blocktrust.Common.Resolver;
using Common.Commands.CreatePeerDid;
using Common.Models.MediatorCoordinator;
using Common.Protocols;
using Connections.CreateConnection;
using Connections.GetConnection;
using Connections.UpdateConnection;
using Connections.UpdateKeyEntries;
using DIDComm.Message.Messages;
using Entities;
using FluentResults;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ProcessUpdateMediatorKeys;

public class ProcessUpdateMediatorKeysHandler : IRequestHandler<ProcessUpdateMediatorKeysRequest, Result<Message>>
{
    private readonly DataContext _context;
    private readonly IMediator _mediator;
    private readonly ISecretResolver _secretResolver;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="context"></param>
    public ProcessUpdateMediatorKeysHandler(DataContext context, IMediator mediator, ISecretResolver secretResolver)
    {
        this._context = context;
        this._mediator = mediator;
        this._secretResolver = secretResolver;
    }

    public async Task<Result<Message>> Handle(ProcessUpdateMediatorKeysRequest request, CancellationToken cancellationToken)
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
            var body = request.UnpackedMessage.Body;
            var hasUpdates = body.TryGetValue("updates", out var updatesBody);
            if (!hasUpdates)
            {
                return Result.Fail("Invalid body");
            }

            var updatesJsonElement = (JsonElement)updatesBody;
            var updatesJson = updatesJsonElement.GetRawText();
            try
            {
                var updates = JsonSerializer.Deserialize<List<KeyListUpdateModel>>(updatesJson);
                var addUpdates = updates.Where(p => p.UpdateType == "add").Select(p => p.KeyToUpdate).ToList();
                var removeUpdates = updates.Where(p => p.UpdateType == "remove").Select(p => p.KeyToUpdate).ToList();
                var updateResult = await _mediator.Send(new UpdateKeyEntriesRequest(request.SenderDid, addUpdates, removeUpdates), cancellationToken);
                if (updateResult.IsFailed)
                {
                    return Result.Fail("Unable to update key entries");
                }
            }
            catch (Exception e)
            {
                return Result.Fail("Invalid body: unable to deserialize updates");
            }

            // Create the message to indicate a successful update
            var mediateGrantMessage = new MessageBuilder(
                    id: Guid.NewGuid().ToString(),
                    type: ProtocolConstants.CoordinateMediation2KeylistUpdateResponse,
                    body: new Dictionary<string, object>()
                    {
                        {"updated", updatesBody}
                    }
                )
                .fromPrior(request.FromPrior)
                .build();
            return Result.Ok(mediateGrantMessage);
        }
    }
}