namespace Blocktrust.Mediator.Server.Commands.OutOfBand.CreateOobInvitation;

using Blocktrust.Mediator.Common.Models.OutOfBand;
using Blocktrust.Mediator.Server;
using Blocktrust.Mediator.Server.Entities;
using Blocktrust.Mediator.Server.Models;
using FluentResults;
using MediatR;

/// <summary>
/// Handler to create a new OOB invitation in the database
/// </summary>
public class CreateOobInvitationHandler : IRequestHandler<CreateOobInvitationRequest, Result<OobInvitationModel>>
{
    private readonly DataContext _context;


    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="context"></param>
    public CreateOobInvitationHandler(DataContext context)
    {
        this._context = context;
    }

    /// <summary>
    /// Handler
    /// </summary>
    /// <param name="createOobInvitationRequest"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<Result<OobInvitationModel>> Handle(CreateOobInvitationRequest createOobInvitationRequest, CancellationToken cancellationToken)
    {
        var invitation = OobModel.BuildRequestMediateMessage(createOobInvitationRequest.PeerDid);

        try
        {
            var oob = new OobInvitationEntity()
            {
                Did = createOobInvitationRequest.PeerDid.Value,
                CreatedUtc = DateTime.UtcNow,
                Url = createOobInvitationRequest.HostUrl,
                Invitation = invitation
            };


            // _context.ChangeTracker.Clear();
            // _context.ChangeTracker.AutoDetectChangesEnabled = false;
            await _context.AddAsync(oob, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            return Result.Ok(new OobInvitationModel(oob));
        }
        catch (Exception e)
        {
            return Result.Fail("Error creating database connection");
        }
    }
}