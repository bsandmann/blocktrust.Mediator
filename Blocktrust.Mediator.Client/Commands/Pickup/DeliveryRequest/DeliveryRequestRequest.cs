namespace Blocktrust.Mediator.Client.Commands.Pickup.DeliveryRequest;

using FluentResults;
using MediatR;

public class DeliveryRequestRequest : IRequest<Result<DeliveryRequestResponse>>
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

    /// <summary>
    /// Optional: a Did to query specifically. When not provides the status returns the messages for all DIDs
    /// </summary>
    public string? RecipientDid { get; }

    /// <summary>
    /// Required. Maximum number of messages to be returned
    /// </summary>
    public int Limit { get; } 


    public DeliveryRequestRequest(string localDid, string mediatorDid, Uri mediatorEndpoint, int limit, string? recipientDid = null)
    {
        LocalDid = localDid;
        MediatorDid = mediatorDid;
        MediatorEndpoint = mediatorEndpoint;
        Limit = limit;
        RecipientDid = recipientDid;
    }
}