namespace Blocktrust.Mediator.Server.Commands.Connections.GetKeyEntries;

using FluentResults;
using MediatR;
using Models;

public class GetKeyEntriesRequest  : IRequest<Result<List<KeyEntryModel>>>
{
    public string RemoteDid { get; }

    public GetKeyEntriesRequest(string remoteDid)
    {
        RemoteDid = remoteDid;
    }
}
