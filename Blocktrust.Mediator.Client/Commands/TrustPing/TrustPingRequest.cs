namespace Blocktrust.Mediator.Client.Commands.TrustPing;

using FluentResults;
using MediatR;

public class TrustPingRequest: IRequest<Result<string?>>
{
    public Uri RemoteEndpoint { get; }
    public string RemoteDid { get; }
    public string LocalDid { get; }
    public bool ResponseRequested { get; }
    public string? SuggestedLabel { get; }

    public TrustPingRequest(Uri remoteEndpoint, string remoteDid, string localDid, bool responseRequested = true, string? suggestedLabel = null)
    {
        RemoteEndpoint = remoteEndpoint;
        RemoteDid = remoteDid;
        LocalDid = localDid;
        ResponseRequested = responseRequested;
        SuggestedLabel = suggestedLabel;
    }
}