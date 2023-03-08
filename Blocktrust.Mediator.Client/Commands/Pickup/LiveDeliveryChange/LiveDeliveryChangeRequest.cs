namespace Blocktrust.Mediator.Client.Commands.Pickup.StatusRequest;

using Common.Models.Pickup;
using Common.Models.ProblemReport;
using FluentResults;
using MediatR;

public class LiveDeliveryChangeRequest : IRequest<Result<ProblemReport>>
{
    /// <summary>
    /// The local did which is used to communicate with the mediator 
    /// </summary>
    public string LocalDid { get; }

    /// <summary>
    /// The DID of the mediator this message goes to
    /// </summary>
    public string MediatorDid { get; }

    public Uri MediatorEndpoint { get; }

    public bool LiveDelivery { get; }


    public LiveDeliveryChangeRequest(string localDid, string mediatorDid, Uri mediatorEndpoint, bool liveDelivery)
    {
        LocalDid = localDid;
        MediatorDid = mediatorDid;
        MediatorEndpoint = mediatorEndpoint;
        LiveDelivery = liveDelivery;
    }
}