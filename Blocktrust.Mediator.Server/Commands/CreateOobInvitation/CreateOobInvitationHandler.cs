﻿namespace Blocktrust.Mediator.Server.Commands.CreateOobInvitation;

using System.Text.Json;
using Server;
using Blocktrust.PeerDID.DIDDoc;
using Blocktrust.PeerDID.PeerDIDCreateResolve;
using Blocktrust.PeerDID.Types;
using Common.Models.OutOfBand;
using Entities;
using FluentResults;
using MediatR;
using Models;

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
}