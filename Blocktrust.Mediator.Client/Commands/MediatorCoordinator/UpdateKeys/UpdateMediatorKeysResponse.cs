namespace Blocktrust.Mediator.Client.Commands.MediatorCoordinator.UpdateKeys;

using Common.Models.ProblemReport;

public class UpdateMediatorKeysResponse
{
    public ProblemReport? ProblemReport { get; }

    public UpdateMediatorKeysResponse(ProblemReport? problemReport = null)
    {
        ProblemReport = problemReport;
    }
}