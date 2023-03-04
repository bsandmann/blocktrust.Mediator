namespace Blocktrust.Mediator.Server.Entities;

public class MediatorConnectionKeyEntity
{
    public Guid MediatorConnectionKeyId { get; set; }
    
    /// <summary>
    /// The Did used by the owner of the connection to communicate with other parties
    /// If Alice and Bob have a relationship-Did, it could be registered here, so that Alice can pick up a message
    /// send from Bob via the mediator to alice
    /// </summary>
    public string Key { get; set; }

    // FK
    public MediatorConnectionEntity MediatorConnectionEntity { get; set; }
}