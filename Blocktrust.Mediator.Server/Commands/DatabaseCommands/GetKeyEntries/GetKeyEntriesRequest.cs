namespace Blocktrust.Mediator.Server.Commands.DatabaseCommands.GetKeyEntries;

using Blocktrust.Mediator.Server.Models;
using FluentResults;
using MediatR;

public class GetKeyEntriesRequest  : IRequest<Result<List<KeyEntryModel>>>
{
    public string RemoteDid { get; }

    public GetKeyEntriesRequest(string remoteDid)
    {
        RemoteDid = remoteDid;
    }
}
