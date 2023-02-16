namespace Blocktrust.Mediator.Commands.CreateOobInvitation;

using Blocktrust.Mediator.Models;
using Blocktrust.PeerDID.Types;
using FluentResults;
using MediatR;

/// <summary>
/// Request to create a new OOB invitation in the database
/// </summary>
public class CreateOobInvitationRequest : IRequest<Result<OobInvitationModel>>
{
    /// <summary>
    /// Request
    /// </summary>
    public CreateOobInvitationRequest(string hostUrl, PeerDid peerDid)
    {
        PeerDid = peerDid;
        HostUrl = hostUrl;
    }

    public string HostUrl { get; }

    public PeerDid PeerDid { get; }
}