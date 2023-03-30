namespace Blocktrust.Mediator.Server.Commands.DatabaseCommands.GetConnection;

using Blocktrust.Mediator.Server.Models;
using Entities;
using FluentResults;
using MediatR;
using Microsoft.EntityFrameworkCore;

public class GetConnectionHandler : IRequestHandler<GetConnectionRequest, Result<MediatorConnectionModel>>
{
    private readonly DataContext _context;


    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="context"></param>
    public GetConnectionHandler(DataContext context)
    {
        this._context = context;
    }

    /// <inheritdoc />
    public async Task<Result<MediatorConnectionModel>> Handle(GetConnectionRequest request, CancellationToken cancellationToken)
    {
        if (request.RemoteDid is null)
        {
            return Result.Ok();
        }
        try
        {
            MediatorConnection? existingConnection;
            if (request.MediatorDid is null)
            {
                existingConnection = await _context.MediatorConnections.FirstOrDefaultAsync(p => request.RemoteDid.Equals(p.RemoteDid), cancellationToken: cancellationToken);
            }
            else
            {
                existingConnection = await _context.MediatorConnections.FirstOrDefaultAsync(p => request.RemoteDid.Equals(p.RemoteDid) && request.MediatorDid.Equals(p.MediatorDid), cancellationToken: cancellationToken);
            }

            if (existingConnection is null)
            {
                return Result.Ok();
            }

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
            return Result.Fail(e.Message);
        }
    }
}