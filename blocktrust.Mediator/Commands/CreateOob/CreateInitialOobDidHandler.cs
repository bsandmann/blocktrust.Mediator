namespace Blocktrust.Mediator.Commands.CreateOob;

using Blocktrust.Mediator;
using Blocktrust.Mediator.Entities;
using Blocktrust.Mediator.Models;
using FluentResults;
using MediatR;

/// <summary>
/// Handler to create new blocks inside the node-database to represent a block
/// </summary>
public class CreateInitialOobDidHandler : IRequestHandler<CreateInitialOobDidRequest, Result<OobModel>>
{
    private readonly DataContext _context;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="context"></param>
    public CreateInitialOobDidHandler(DataContext context)
    {
        this._context = context;
    }

    /// <summary>
    /// Handler
    /// </summary>
    /// <param name="didRequest"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<Result<OobModel>> Handle(CreateInitialOobDidRequest didRequest, CancellationToken cancellationToken)
    {
        // Create 1 key pairs for aggreement keys (X25519)
        //  one public with crv, x, kty, kid
        // one private with crv, x, kty, kid, d
        
        // Create 1 key pair for authentication (signing) ED25519
        // one public with crv, x, kty, kid
        // one private with crv, x, kty, kid, d
        
        // _context.ChangeTracker.Clear();
        // _context.ChangeTracker.AutoDetectChangesEnabled = false;
        var now = DateTime.UtcNow;

        var oob = new OobEntity()
        {
            Did = didRequest.Did,
            CreatedUtc = now,
            Url = "https://www.google.com"
        };
        await _context.AddAsync(oob, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Ok(new OobModel(oob));
    }
}