namespace Blocktrust.Mediator.Server.Commands.Connections.CreateConnection;

using Entities;
using FluentResults;
using MediatR;

public class CreateConnectionHandler : IRequestHandler<CreateConnectionRequest, Result>
{
    private readonly DataContext _context;


    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="context"></param>
    public CreateConnectionHandler(DataContext context)
    {
        this._context = context;
    }

    public async Task<Result> Handle(CreateConnectionRequest request, CancellationToken cancellationToken)
    {
        await _context.Connections.AddAsync(new MediatorConnectionEntity()
        {
            MediatorDid = request.MediatorDid,
            RemoteDid = request.RemoteDid,
            CreatedUtc = DateTime.UtcNow,
            MediationGranted = false,
            RoutingDid = null,
            MediatorEndpoint = null
        }, cancellationToken);

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
            return Result.Ok();
        }
        catch (Exception e)
        {
            return Result.Fail("Error establishing database connection");
        }
    }
}