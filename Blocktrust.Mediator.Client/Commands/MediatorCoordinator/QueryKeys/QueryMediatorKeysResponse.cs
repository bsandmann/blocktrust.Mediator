namespace Blocktrust.Mediator.Client.Commands.MediatorCoordinator.QueryKeys;

using Common.Models.ProblemReport;

public class QueryMediatorKeysResponse
{
    public ProblemReport? ProblemReport { get; }
    
    public List<string>? RegisteredMediatorKeys { get; }

    public QueryMediatorKeysResponse(ProblemReport problemReport)
    {
        ProblemReport = problemReport;
    } 
    public QueryMediatorKeysResponse(List<string> registeredMediatorKeys)
    {
        RegisteredMediatorKeys = registeredMediatorKeys;
    } 
}