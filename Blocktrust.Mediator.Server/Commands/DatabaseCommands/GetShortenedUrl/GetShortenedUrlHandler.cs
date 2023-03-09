namespace Blocktrust.Mediator.Server.Commands.DatabaseCommands.GetShortenedUrl;

using Blocktrust.Mediator.Server;
using FluentResults;
using MediatR;

/// <summary>
/// Handler to create a new OOB invitation in the database
/// </summary>
public class GetShortenedUrlHandler : IRequestHandler<GetShortenedUrlRequest, Result<string>>
{
    private readonly DataContext _context;


    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="context"></param>
    public GetShortenedUrlHandler(DataContext context)
    {
        this._context = context;
    }

    /// <summary>
    /// Handler
    /// </summary>
    /// <param name="createShortenedUrlRequest"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<Result<string>> Handle(GetShortenedUrlRequest createShortenedUrlRequest, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _context.ShortenedUrlEntities.FindAsync(createShortenedUrlRequest.ShortenedUrlEntityId);

            if (result is null)
            {
                return Result.Fail("Requested shortened URL could not be found");
            }

            if (result.ExpirationUtc < DateTime.UtcNow)
            {
                return Result.Fail("Requested shortened URL is not valid anymore");
            }

            return Result.Ok(result.LongFormUrl);
        }
        catch (Exception e)
        {
            return Result.Fail("Error creating database connection");
        }
    }
}