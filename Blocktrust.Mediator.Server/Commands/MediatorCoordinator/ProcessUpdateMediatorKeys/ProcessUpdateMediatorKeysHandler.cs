namespace Blocktrust.Mediator.Server.Commands.MediatorCoordinator.ProcessUpdateMediatorKeys;

using System.Text.Json;
using Blocktrust.DIDComm.Message.Messages;
using Blocktrust.Mediator.Common.Models.MediatorCoordinator;
using Blocktrust.Mediator.Common.Protocols;
using DatabaseCommands.GetConnection;
using DatabaseCommands.UpdateRegisteredRecipients;
using FluentResults;
using MediatR;

public class ProcessUpdateMediatorKeysHandler : IRequestHandler<ProcessUpdateMediatorKeysRequest, Result<Message>>
{
    private readonly IMediator _mediator;

    /// <summary>
    /// Constructor
    /// </summary>
    public ProcessUpdateMediatorKeysHandler(IMediator mediator)
    {
        this._mediator = mediator;
    }

    public async Task<Result<Message>> Handle(ProcessUpdateMediatorKeysRequest request, CancellationToken cancellationToken)
    {
        var existingConnection = await _mediator.Send(new GetConnectionRequest(request.SenderDid, request.MediatorDid), cancellationToken);
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
                var updateResult = await _mediator.Send(new UpdateRegisteredRecipientsRequest(request.SenderDid, addUpdates, removeUpdates), cancellationToken);
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