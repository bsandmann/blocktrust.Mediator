namespace Blocktrust.Mediator.Server.Commands.DatabaseCommands.GetConnection;

using Blocktrust.Mediator.Server.Models;
using FluentResults;
using MediatR;

public class GetConnectionRequest  : IRequest<Result<MediatorConnectionModel>>
{
    public string RemoteDid { get; }

    public GetConnectionRequest(string remoteDid)
    {
        RemoteDid = remoteDid;
    }
}