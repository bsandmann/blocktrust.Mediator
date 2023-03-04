namespace Blocktrust.Mediator.Client.Commands.MediatorCoordinator.RequestMediation;

public class RequestMediationResponse
{
    public bool MediationGranted { get; set; }

    public string MediatorDid { get; set; }
    public Uri MediatorEndpoint { get; set; }

    public string? RoutingDid { get; set; }

    public RequestMediationResponse(string mediatorDid, Uri mediatorEndpoint, string routingDid)
    {
        this.RoutingDid = routingDid;
        this.MediatorDid = mediatorDid;
        this.MediatorEndpoint = mediatorEndpoint;
        this.MediationGranted = true;
    }

    public RequestMediationResponse()
    {
        MediationGranted = false;
    }
}