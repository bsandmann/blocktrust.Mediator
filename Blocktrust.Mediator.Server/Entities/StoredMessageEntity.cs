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
    /// The Id of the message as a selector for deletion
    /// </summary>
    public string MessageId { get; set; }

    /// <summary>
    /// The hash of the message. For finding duplicates
    /// </summary>
    public string MessageHash { get; set; }

    /// <summary>
    /// The attachment of the message. Should be encrypted
    /// </summary>
    public string Message { get; set; }
    
    /// <summary>
    /// Size of the Message in bytes
    /// </summary>
    public long MessageSize { get; set; }

    /// <summary>
    /// FK
    /// </summary>
    public ConnectionKeyEntity ConnectionKeyEntity { get; set; }
}