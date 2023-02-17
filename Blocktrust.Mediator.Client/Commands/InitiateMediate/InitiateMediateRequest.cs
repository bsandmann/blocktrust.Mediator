namespace Blocktrust.Mediator.Client.Commands.InitiateMediate;

using FluentResults;
using MediatR;

public class InitiateMediateRequest : IRequest<Result<string>>
{
    public string OobInvitation { get; }

    public InitiateMediateRequest(string oobInvitation)
    {
        this.OobInvitation = oobInvitation;
    }
}