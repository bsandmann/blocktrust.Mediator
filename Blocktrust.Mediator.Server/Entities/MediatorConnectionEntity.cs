namespace Blocktrust.Mediator.Server.Entities;

public class MediatorConnectionEntity
{
    public Guid MediatorConnectionId { get; set; }
    public string MediatorDid { get; set; }
    public string RemoteDid { get; set; }
    public string? RoutingDid { get; set; }
    public string? MediatorEndpoint { get; set; }
    public bool MediationGranted { get; set; }
    public DateTime CreatedUtc { get; set; }

    // there are other entries like
    // routing_did
    // update time k
}