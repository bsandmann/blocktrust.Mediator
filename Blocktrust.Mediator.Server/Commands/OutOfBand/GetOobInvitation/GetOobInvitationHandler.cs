namespace Blocktrust.Mediator.Server.Commands.OutOfBand.GetOobInvitation;

using Blocktrust.Mediator.Server;
using Blocktrust.Mediator.Server.Models;
using FluentResults;
using MediatR;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// Handler to create a new OOB invitation in the database
/// </summary>
public class GetOobInvitationHandler : IRequestHandler<GetOobInvitationRequest, Result<OobInvitationModel>>
{
    private readonly DataContext _context;


    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="context"></param>
    public GetOobInvitationHandler(DataContext context)
    {
        this._context = context;
    }

    /// <summary>
    /// Handler
    /// </summary>
    /// <param name="getOobInvitationRequest"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<Result<OobInvitationModel>> Handle(GetOobInvitationRequest getOobInvitationRequest, CancellationToken cancellationToken)
    {
        var invitation = await _context.OobInvitations.FirstOrDefaultAsync(p => p.Url == getOobInvitationRequest.HostUrl.ToLowerInvariant(), cancellationToken);
        if (invitation == null)
        {
            return Result.Fail("No invitation found. Create a new one for the url");
        }

        return Result.Ok(new OobInvitationModel(invitation));
    }
}