namespace Blocktrust.Mediator.Server.Commands.DatabaseCommands.GetConnection;

using Blocktrust.Mediator.Server.Models;
using FluentResults;
using MediatR;

public class GetConnectionRequest : IRequest<Result<MediatorConnectionModel>>
{
    public string RemoteDid { get; }
    public string? MediatorDid { get; }

    public GetConnectionRequest(string remoteDid, string? mediatorDid)
    {
        RemoteDid = remoteDid;
        MediatorDid = mediatorDid;
    }
}