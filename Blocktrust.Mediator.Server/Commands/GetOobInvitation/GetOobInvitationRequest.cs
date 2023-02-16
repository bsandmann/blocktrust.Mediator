namespace Blocktrust.Mediator.Server.Commands.GetOobInvitation;

using FluentResults;
using MediatR;
using Models;

/// <summary>
/// Request to retrieve a OOB invitation from the database
/// </summary>
public class GetOobInvitationRequest : IRequest<Result<OobInvitationModel>>
{
    /// <summary>
    /// Request
    /// </summary>
    public GetOobInvitationRequest(string hostUrl)
    {
        HostUrl = hostUrl;
    }

    /// <summary>
    /// The url the mediator is currently running on e.g. 'https://localhost:5001'
    /// </summary>
    public string HostUrl { get; }
}