namespace Blocktrust.Mediator.Client.Commands.MediatorCoordinator.InquireMediation;

using FluentResults;
using MediatR;

public class InquireMediationRequest : IRequest<Result<InquireMediationResponse>>
{
    /// <summary>
    /// The invitation from the mediator
    /// </summary>
    public string OobInvitation { get; }
    
    /// <summary>
    /// The local did to be send to the mediator to register it
    /// </summary>
    public string LocalDid { get; }

    public InquireMediationRequest(string oobInvitation, string localDid)
    {
        this.OobInvitation = oobInvitation;
        this.LocalDid = localDid;
    }
}