namespace Blocktrust.Mediator.Server.Commands.DatabaseCommands.UpdateConnection;

using Blocktrust.Mediator.Server.Models;
using FluentResults;
using MediatR;
using Microsoft.EntityFrameworkCore;

public class UpdateConnectionMediationHandler : IRequestHandler<UpdateConnectionMediationRequest, Result<MediatorConnectionModel>>
{
    private readonly DataContext _context;


    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="context"></param>
    public UpdateConnectionMediationHandler(DataContext context)
    {
        this._context = context;
    }

    public async Task<Result<MediatorConnectionModel>> Handle(UpdateConnectionMediationRequest request, CancellationToken cancellationToken)
    {
        try
        {
            _context.ChangeTracker.Clear();
            _context.ChangeTracker.AutoDetectChangesEnabled = false;
            var existingConnection = await _context.MediatorConnections.FirstOrDefaultAsync(p => p.MediatorDid.Equals(request.MediatorDid) && p.RemoteDid.Equals(request.RemoteDid), cancellationToken);

            if (existingConnection is null)
            {
                Result.Fail("The connection does not exist. That should not happen at this stage.");
            }

            existingConnection.MediationGranted = request.MediationGranted;
            existingConnection.RoutingDid = request.RoutingDid;
            existingConnection.MediatorEndpoint = request.MediatorEndpoint;

            _context.Update(existingConnection);
            await _context.SaveChangesAsync(cancellationToken);
            return Result.Ok(new MediatorConnectionModel(
                mediatorDid: existingConnection.MediatorDid,
                remoteDid: existingConnection.RemoteDid,
                routingDid: existingConnection.RoutingDid,
                mediatorEndpoint: existingConnection.MediatorEndpoint,
                mediationGranted: existingConnection.MediationGranted,
                createdUtc: existingConnection.CreatedUtc
            ));
        }
        catch (Exception e)
        {
            return Result.Fail("Error establishing database connection");
        }
    }
}