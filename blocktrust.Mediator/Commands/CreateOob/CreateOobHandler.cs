namespace Blocktrust.Node.Commands.CreateBlock;

using FluentResults;
using Mediator;
using Mediator.Entities;
using Mediator.Models;
using MediatR;

/// <summary>
/// Handler to create new blocks inside the node-database to represent a block
/// </summary>
public class CreateOobHandler : IRequestHandler<CreateOobRequest, Result<OobModel>>
{
    private readonly DataContext _context;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="context"></param>
    public CreateOobHandler(DataContext context)
    {
        this._context = context;
    }

    /// <summary>
    /// Handler
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<Result<OobModel>> Handle(CreateOobRequest request, CancellationToken cancellationToken)
    {
        // _context.ChangeTracker.Clear();
        // _context.ChangeTracker.AutoDetectChangesEnabled = false;
        var now = DateTime.UtcNow;

        var oob = new OobEntity()
        {
            OobId = Guid.NewGuid(),
            Did = request.Did,
            CreatedUtc = now,
            Url = "https://www.google.com"
        };
        await _context.AddAsync(oob, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Ok(new OobModel(oob));
    }
}