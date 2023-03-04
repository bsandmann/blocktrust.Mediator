namespace Blocktrust.Mediator.Client.Commands.MediatorCoordinator.RequestMediation;

using FluentResults;
using MediatR;

public class RequestMediationRequest : IRequest<Result<RequestMediationResponse>>
{
    /// <summary>
    /// The invitation from the mediator
    /// </summary>
    public string OobInvitation { get; }
    
    /// <summary>
    /// The local did to be send to the mediator to register it
    /// </summary>
    public string LocalDid { get; }

    public RequestMediationRequest(string oobInvitation, string localDid)
    {
        this.OobInvitation = oobInvitation;
        this.LocalDid = localDid;
    }
}