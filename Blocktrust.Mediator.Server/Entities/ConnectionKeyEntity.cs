namespace Blocktrust.Mediator.Server.Entities;

public class ConnectionKeyEntity
{
    public Guid ConnectionKeyEntityId { get; set; }

    /// <summary>
    /// The Did used by the owner of the connection to communicate with other parties
    /// If Alice and Bob have a relationship-Did, it could be registered here, so that Alice can pick up a message
    /// send from Bob via the mediator to alice
    /// </summary>
    public string RecipientKey { get; set; }

    /// <summary>
    /// Messages that are stored for this key
    /// </summary>
    public List<StoredMessageEntity> StoredMessage { get; set; }

    // FK
    public ConnectionEntity ConnectionEntity { get; set; }
}