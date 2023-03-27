namespace Blocktrust.Mediator.Client.Commands.TrustPing;

using FluentResults;
using MediatR;

public class TrustPingRequest: IRequest<Result>
{
    public Uri RemoteEndpoint { get; }
    public string RemoteDid { get; }
    public string LocalDid { get; }
    public bool ResponseRequested { get; }

    public TrustPingRequest(Uri remoteEndpoint, string remoteDid, string localDid, bool responseRequested = true)
    {
        RemoteEndpoint = remoteEndpoint;
        RemoteDid = remoteDid;
        LocalDid = localDid;
        ResponseRequested = responseRequested;
    }
}