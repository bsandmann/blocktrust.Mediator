namespace Blocktrust.Mediator.Server.Commands.DatabaseCommands.UpdateRegisteredRecipients;

using FluentResults;
using MediatR;

/// <inheritdoc />
public class UpdateRegisteredRecipientsRequest : IRequest<Result>
{
    public string RemoteDid { get; }
    public List<string> KeysToAdd { get; }
    public List<string> KeysToRemove { get; }

    public UpdateRegisteredRecipientsRequest(string remoteDid, List<string> keysToAdd, List<string> keysToRemove)
    {
        RemoteDid = remoteDid;
        KeysToAdd = keysToAdd;
        KeysToRemove = keysToRemove;
    }
}