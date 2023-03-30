namespace Blocktrust.Mediator.Server.Commands.DatabaseCommands.InvalidateShortenedUrl;

using Blocktrust.Mediator.Server;
using FluentResults;
using MediatR;

/// <summary>
/// Handler to delete a shortened Url entry in the database 
/// </summary>
public class InvalidateShortenedUrlHandler : IRequestHandler<InvalidateShortenedUrlRequest, Result>
{
    private readonly DataContext _context;


    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="context"></param>
    public InvalidateShortenedUrlHandler(DataContext context)
    {
        this._context = context;
    }

    /// <summary>
    /// Handler
    /// </summary>
    /// <param name="invalidateShortenedUrlRequest"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<Result> Handle(InvalidateShortenedUrlRequest invalidateShortenedUrlRequest, CancellationToken cancellationToken)
    {
        try
        {
            var shortenedUrlEntity = await _context.ShortenedUrlEntities.FindAsync(invalidateShortenedUrlRequest.ShortenedUrlEntityId);
            if (shortenedUrlEntity != null)
            {
                _context.ShortenedUrlEntities.Remove(shortenedUrlEntity);
                await _context.SaveChangesAsync(cancellationToken);
            }

            return Result.Ok();
        }
        catch (Exception e)
        {
            return Result.Fail("Error creating database connection");
        }
    }
}