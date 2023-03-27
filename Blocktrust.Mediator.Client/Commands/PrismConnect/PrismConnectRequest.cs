namespace Blocktrust.Mediator.Client.Commands.PrismConnect;

using FluentResults;
using MediatR;

public class PrismConnectRequest: IRequest<Result<string>>
{
    public Uri RemoteEndpoint { get; }
    public string RemoteDid { get; }
    public string LocalDid { get; }

    public PrismConnectRequest(Uri remoteEndpoint, string remoteDid, string localDid)
    {
        RemoteEndpoint = remoteEndpoint;
        RemoteDid = remoteDid;
        LocalDid = localDid;
    }
}