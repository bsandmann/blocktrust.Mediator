namespace Blocktrust.Mediator.Server.Commands.Connections.GetConnection;

using FluentResults;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Models;

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

    public async Task<Result<MediatorConnectionModel>> Handle(GetConnectionRequest request, CancellationToken cancellationToken)
    {
        try
        {
            
            //TODO we ahould ask for the mediator AND the remotedid here!
            var existingConnection = await _context.Connections.FirstOrDefaultAsync(p => request.RemoteDid.Equals(p.RemoteDid), cancellationToken: cancellationToken);
            if (existingConnection is null)
            {
                return Result.Ok<MediatorConnectionModel>(null);
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
            return Result.Fail<MediatorConnectionModel>(e.Message);
        }
    }
}