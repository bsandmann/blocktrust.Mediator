namespace Blocktrust.Mediator.Client.Commands.ForwardMessage;

using FluentResults;
using MediatR;

public class SendForwardMessageRequest : IRequest<Result>
{
    /// <summary>
    /// The Message to be forwarded 
    /// </summary>
    public string Message { get; }
    
    /// <summary>
    /// The local did who send the message (e.g. a DID that is created to be used solely for the communication with the mediator of the other party)
    /// </summary>
    public string LocalDid { get; }

    /// <summary>
    /// The DID of the mediator this message goes to
    /// </summary>
    public string MediatorDid { get; }
    
    /// <summary>
    /// THe final recipient, that is the person who registered the mediator this message is send to
    /// </summary>
    public string RecipientDid { get; }
    
    public Uri MediatorEndpoint { get; }


    public SendForwardMessageRequest(string message, string localDid, string mediatorDid, string recipientDid, Uri mediatorEndpoint)
    {
        Message = message;
        LocalDid = localDid;
        MediatorDid = mediatorDid;
        RecipientDid = recipientDid;
        MediatorEndpoint = mediatorEndpoint;
    }
}