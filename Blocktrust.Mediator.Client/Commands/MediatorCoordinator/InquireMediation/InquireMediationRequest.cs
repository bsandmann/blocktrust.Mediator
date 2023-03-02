namespace Blocktrust.Mediator.Client.Commands.MediatorCoordinator.InquireMediation;

using FluentResults;
using MediatR;

public class InquireMediationRequest : IRequest<Result<InquireMediationResponse>>
{
    public string OobInvitation { get; }

    public InquireMediationRequest(string oobInvitation)
    {
        this.OobInvitation = oobInvitation;
    }
}