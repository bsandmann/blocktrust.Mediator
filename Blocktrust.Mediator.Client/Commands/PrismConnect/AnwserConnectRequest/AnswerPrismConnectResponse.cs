namespace Blocktrust.Mediator.Client.Commands.PrismConnect.AnwserConnectRequest;

using Common.Models.ProblemReport;

public class AnswerPrismConnectResponse
{
    public ProblemReport? ProblemReport { get; }
    public string? MessageIdOfResponse { get; }
    
    public string? PrismDid { get; }

    public AnswerPrismConnectResponse(string messageIdOfResponse, string prismDid)
    {
        MessageIdOfResponse = messageIdOfResponse;
        PrismDid = prismDid;
    }

    public AnswerPrismConnectResponse(ProblemReport problemReport)
    {
        ProblemReport = problemReport;
    }
}