namespace Blocktrust.Mediator.Client.Commands.TrustPing;

using FluentResults;
using MediatR;

public class TrustPingRequest: IRequest<Result>
{
    public Uri MediatorEndpoint { get; }
    public string MediatorDid { get; }
    public string LocalDid { get; }
    public bool ResponseRequested { get; }

    public TrustPingRequest(Uri mediatorEndpoint, string mediatorDid, string localDid, bool responseRequested = true)
    {
        MediatorEndpoint = mediatorEndpoint;
        MediatorDid = mediatorDid;
        LocalDid = localDid;
        ResponseRequested = responseRequested;
    }
}