namespace Blocktrust.Mediator.Server.Entities;

public class StoredMessageEntity
{
    /// <summary>
    /// Id
    /// </summary>
    public Guid StoredMessageEntityId { get; set; }

    /// <summary>
    /// Timestamp when the message was stored here
    /// </summary>
    public DateTime Created { get; set; }

    /// <summary>
    /// The attachment of the message. Should be encrypted
    /// </summary>
    public string MessageId { get; set; }

    /// <summary>
    /// The attachment of the message. Should be encrypted
    /// </summary>
    public string MessageHash { get; set; }

    /// <summary>
    /// The attachment of the message. Should be encrypted
    /// </summary>
    public string Message { get; set; }

    /// <summary>
    /// FK
    /// </summary>
    public ConnectionKeyEntity ConnectionKeyEntity { get; set; }
}