namespace Blocktrust.Mediator.Client.Commands.Pickup.DelieveryRequest;

using Common.Models.Pickup;
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
    public string MediatorDid { get; set; }
    
    public Uri MediatorEndpoint { get; set; }

    /// <summary>
    /// Optional: a Did to query specifically. When not provides the status returns the messages for all DIDs
    /// </summary>
    public string? RecipientDid { get; set; }

    /// <summary>
    /// Required. Maximum number of messages to be returned
    /// </summary>
    public int Limit { get; set; } 


    public DeliveryRequestRequest(string localDid, string mediatorDid, Uri mediatorEndpoint, int limit, string? recipientDid = null)
    {
        LocalDid = localDid;
        MediatorDid = mediatorDid;
        MediatorEndpoint = mediatorEndpoint;
        Limit = limit;
        RecipientDid = recipientDid;
    }
}