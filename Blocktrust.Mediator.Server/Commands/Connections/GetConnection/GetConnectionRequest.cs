namespace Blocktrust.Mediator.Server.Commands.Connections.GetConnection;

using FluentResults;
using MediatR;
using Models;

public class GetConnectionRequest  : IRequest<Result<MediatorConnectionModel>>
{
    public string RemoteDid { get; }

    public GetConnectionRequest(string remoteDid)
    {
        RemoteDid = remoteDid;
    }
}