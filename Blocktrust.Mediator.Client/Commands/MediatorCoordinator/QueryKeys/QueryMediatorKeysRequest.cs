namespace Blocktrust.Mediator.Client.Commands.MediatorCoordinator.QueryKeys;

using FluentResults;
using MediatR;

public class QueryMediatorKeysRequest : IRequest<Result<QueryMediatorKeysResponse>>
{
    public Uri MediatorEndpoint { get; }
    public string MediatorDid { get; }
    public string LocalDid { get; }

    public QueryMediatorKeysRequest(Uri mediatorEndpoint, string mediatorDid, string localDid)
    {
        MediatorEndpoint = mediatorEndpoint;
        MediatorDid = mediatorDid;
        LocalDid = localDid;
    }
}