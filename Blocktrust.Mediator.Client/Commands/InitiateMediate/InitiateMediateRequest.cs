namespace Blocktrust.Mediator.Client.Commands.InitiateMediate;

using FluentResults;
using MediatR;

public class InitiateMediateRequest : IRequest<Result>
{
    public string OobInvitation { get; }

    public InitiateMediateRequest(string oobInvitation)
    {
        this.OobInvitation = oobInvitation;
    }
}