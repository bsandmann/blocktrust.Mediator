namespace Blocktrust.Mediator.Server.Models;

public class KeyEntryModel
{
    /// <summary>
    /// The controlling DID
    /// </summary>
    public string RemoteDid { get; set; }
    
    /// <summary>
    /// The key-entry; the DID of two parties that have a relationship
    /// </summary>
    public string KeyEntry { get; set; }
}