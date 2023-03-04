namespace Blocktrust.Mediator.Server.Commands.Connections.UpdateKeyEntries;

using FluentResults;
using MediatR;

public class UpdateKeyEntriesRequest : IRequest<Result>
{
    public string RemoteDid { get; }
    public List<string> KeysToAdd { get; }
    public List<string> KeysToRemove { get; }

    public UpdateKeyEntriesRequest(string remoteDid, List<string> keysToAdd, List<string> keysToRemove)
    {
        RemoteDid = remoteDid;
        KeysToAdd = keysToAdd;
        KeysToRemove = keysToRemove;
    }
}