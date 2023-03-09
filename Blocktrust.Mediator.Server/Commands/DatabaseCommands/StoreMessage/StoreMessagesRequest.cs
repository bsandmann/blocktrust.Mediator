namespace Blocktrust.Mediator.Server.Commands.DatabaseCommands.StoreMessage;

using FluentResults;
using MediatR;
using Models;

public class StoreMessagesRequest : IRequest<Result>
{
    /// <summary>
    /// The DID of the mediator storing the message
    /// </summary>
    public string MediatorDid { get; }

    /// <summary>
    /// The Key-Entry of the recipient in the connections of a DID which uses the mediator
    /// The sender sends a message not directly to the mediator, but to this special DID the recipient has
    /// explicitly registered with the mediator
    /// </summary>
    public string RecipientDid { get; }

    /// <summary>
    /// The message that should be store. Currently only JSON is supported
    /// </summary>
    public List<StoredMessageModel> Messages { get; }

    public StoreMessagesRequest(string mediatorDid, string recipientDid, List<StoredMessageModel> message)
    {
        MediatorDid = mediatorDid;
        RecipientDid = recipientDid;
        Messages = message;
    }
}