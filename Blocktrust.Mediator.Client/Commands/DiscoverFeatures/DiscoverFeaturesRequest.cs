namespace Blocktrust.Mediator.Client.Commands.DiscoverFeatures;

using Common.Models.DiscoverFeatures;
using FluentResults;
using MediatR;

public class DiscoverFeaturesRequest: IRequest<Result<List<DiscoverFeature>>>
{
    public Uri MediatorEndpoint { get; }
    public string MediatorDid { get; }
    public string LocalDid { get; }
    
    public List<FeatureQuery> Queries { get; }

    public DiscoverFeaturesRequest(Uri mediatorEndpoint, string mediatorDid, string localDid, List<FeatureQuery> queries)
    {
        MediatorEndpoint = mediatorEndpoint;
        MediatorDid = mediatorDid;
        LocalDid = localDid;
        Queries = queries;
    }
}