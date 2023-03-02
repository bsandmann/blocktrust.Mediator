namespace Blocktrust.Mediator.Server.Models;

public class MediatorConnectionModel
{
    public string MediatorDid { get; set; }
    public string RemoteDid { get; set; }
    public string? RoutingDid { get; set; }
    public string? MediatorEndpoint { get; set; }
    public bool MediationGranted { get; set; }
    public DateTime CreatedUtc { get; set; }

    public MediatorConnectionModel(string mediatorDid, string remoteDid, string? routingDid, string? mediatorEndpoint, bool mediationGranted, DateTime createdUtc)
    {
        MediatorDid = mediatorDid;
        RemoteDid = remoteDid;
        RoutingDid = routingDid;
        MediatorEndpoint = mediatorEndpoint;
        MediationGranted = mediationGranted;
        CreatedUtc = createdUtc;
    }
}