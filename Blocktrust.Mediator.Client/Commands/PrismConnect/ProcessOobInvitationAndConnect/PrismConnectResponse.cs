namespace Blocktrust.Mediator.Client.Commands.PrismConnect.ProcessOobInvitationAndConnect;

using Blocktrust.Mediator.Common.Models.ProblemReport;

public class PrismConnectResponse
{
    public ProblemReport? ProblemReport { get; }
    public string? MessageIdOfResponse { get; }
    
    public string? PrismDid { get; }

    public PrismConnectResponse(string messageIdOfResponse, string prismDid)
    {
        MessageIdOfResponse = messageIdOfResponse;
        PrismDid = prismDid;
    }

    public PrismConnectResponse(ProblemReport problemReport)
    {
        ProblemReport = problemReport;
    }
}