namespace Blocktrust.Mediator.Client.Commands.Pickup.MessageReceived;

using Common.Models.Pickup;
using FluentResults;
using MediatR;

public class MessageReceivedRequest: IRequest<Result<StatusRequestResponse>>
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
    /// List of messages which have been received be the client and could now be deleted from the mediator 
    /// </summary>
    public List<string> MessageIds { get; }


    public MessageReceivedRequest(string localDid, string mediatorDid, Uri mediatorEndpoint, List<string> messageIds)
    {
        LocalDid = localDid;
        MediatorDid = mediatorDid;
        MediatorEndpoint = mediatorEndpoint;
        MessageIds = messageIds;
    }
}