namespace Blocktrust.Mediator.Client.Commands.MediatorCoordinator.RequestMediation;

using Common.Models.ProblemReport;

public class RequestMediationResponse
{
    public bool MediationGranted { get; set; }

    public string MediatorDid { get; set; }
    public Uri MediatorEndpoint { get; set; }

    public string? RoutingDid { get; set; }
    
    public ProblemReport? ProblemReport { get; set; }

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

    public RequestMediationResponse(ProblemReport problemReport)
    {
        ProblemReport = problemReport;
    }
}