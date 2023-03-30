namespace Blocktrust.Mediator.Client.Commands.PrismConnect;

using FluentResults;
using MediatR;

public class PrismConnectRequest : IRequest<Result<PrismConnectResponse>>
{
    public Uri PrismEndpoint { get; }
    public string PrismDid { get; }
    public string LocalDidToUseWithPrism { get; }
    public string ThreadId { get; }

    public Uri? MediatorEndpoint { get; }
    public string? MediatorDid { get; }
    public string? LocalDidToUseWithMediator { get; }

    public PrismConnectRequest(Uri prismEndpoint, string prismDid, string localDidToUseWithPrism, string threadId, Uri? mediatorEndpoint, string? localDidToUseWithMediator, string? mediatorDid)
    {
        PrismEndpoint = prismEndpoint;
        PrismDid = prismDid;
        LocalDidToUseWithPrism = localDidToUseWithPrism;
        ThreadId = threadId;

        MediatorEndpoint = mediatorEndpoint;
        LocalDidToUseWithMediator = localDidToUseWithMediator;
        MediatorDid = mediatorDid;
    }
}