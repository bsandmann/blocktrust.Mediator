namespace Blocktrust.Mediator.Server.Entities;

public class MediatorConnection
{
    /// <summary>
    /// Id
    /// </summary>
    public Guid MediatorConnectionId { get; set; }
    
    /// <summary>
    /// The did of the mediator for this connection
    /// </summary>
    public string MediatorDid { get; set; }
    
    /// <summary>
    /// The other end of the connection, usually a clients DID
    /// </summary>
    public string RemoteDid { get; set; }
    
    /// <summary>
    /// The routing did of the mediator. This is used to route messages to the mediator
    /// </summary>
    public string? RoutingDid { get; set; }
    
    /// <summary>
    /// The endpoint of the mediator
    /// </summary>
    public string? MediatorEndpoint { get; set; }
    
    /// <summary>
    /// Flag if the mediation is granted / enabled
    /// </summary>
    public bool MediationGranted { get; set; }
    
    /// <summary>
    /// When the connection was established
    /// </summary>
    public DateTime CreatedUtc { get; set; }
    
    public List<RegisteredRecipient> RegisteredRecipients { get; set; }
}