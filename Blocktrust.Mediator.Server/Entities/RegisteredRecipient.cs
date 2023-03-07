namespace Blocktrust.Mediator.Server.Entities;

public class RegisteredRecipient
{
    /// <summary>
    /// The Did used by the owner of the connection to communicate with other parties
    /// If Alice and Bob have a relationship-Did, it could be registered here, so that Alice can pick up a message
    /// send from Bob via the mediator to alice
    /// </summary>
    public string RecipientDid { get; set; }

    /// <summary>
    /// Messages that are stored for this key
    /// </summary>
    public List<StoredMessage> StoredMessage { get; set; }

    /// <summary>
    /// FK
    /// </summary>
    public MediatorConnection MediatorConnection { get; set; }
    
    /// <summary>
    /// FK
    /// </summary>
    public Guid MediatorConnectionId { get; set; }
}