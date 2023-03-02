namespace Blocktrust.Mediator.Client.Commands.MediatorCoordinator.InquireMediation;

public class InquireMediationResponse
{
    public bool MediationGranted { get; set; }
    
    public string? RoutingDid { get; set; }

    public InquireMediationResponse(string routingDid)
    {
        this.RoutingDid = routingDid;
        this.MediationGranted = true;
    }

    public InquireMediationResponse()
    {
        MediationGranted = false;
    }
}