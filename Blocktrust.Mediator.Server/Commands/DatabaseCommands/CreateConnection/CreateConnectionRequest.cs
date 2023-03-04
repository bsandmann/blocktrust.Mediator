namespace Blocktrust.Mediator.Server.Commands.Connections.CreateConnection;

using FluentResults;
using MediatR;

public class CreateConnectionRequest : IRequest<Result>
{
    public string MediatorDid { get; }
    public string RemoteDid { get; }
    public string CurrentMediatorEndpoint { get; }

    public CreateConnectionRequest(string mediatorDid, string remoteDid)
    {
        MediatorDid = mediatorDid;
        RemoteDid = remoteDid;
    }
}