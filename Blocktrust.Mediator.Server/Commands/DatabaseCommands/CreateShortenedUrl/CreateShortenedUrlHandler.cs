namespace Blocktrust.Mediator.Server.Commands.DatabaseCommands.CreateShortenedUrl;

using Blocktrust.Mediator.Server;
using Blocktrust.Mediator.Server.Entities;
using Blocktrust.Mediator.Server.Models;
using Common.Models.ShortenUrl;
using FluentResults;
using MediatR;

/// <summary>
/// Handler to create a new OOB invitation in the database
/// </summary>
public class CreateShortenedUrlHandler : IRequestHandler<CreateShortenedUrlRequest, Result<string>>
{
    private readonly DataContext _context;


    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="context"></param>
    public CreateShortenedUrlHandler(DataContext context)
    {
        this._context = context;
    }

    /// <summary>
    /// Handler
    /// </summary>
    /// <param name="createShortenedUrlRequest"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<Result<string>> Handle(CreateShortenedUrlRequest createShortenedUrlRequest, CancellationToken cancellationToken)
    {
        try
        {
            var guid = Guid.NewGuid();
            DateTime? expirationUtc = null;
            if (createShortenedUrlRequest.RequestValidityInSeconds is not null && createShortenedUrlRequest.RequestValidityInSeconds > 0)
            {
                expirationUtc = DateTime.UtcNow.AddSeconds((double)createShortenedUrlRequest.RequestValidityInSeconds);
            }

            var goalCode = string.Empty;
            if (createShortenedUrlRequest.GoalCode == EnumShortenUrlGoalCode.ShortenOOBv2)
            {
                goalCode = "shorten.oobv2";
            }
            else
            {
                return Result.Fail("Internal error");
            }

            var existingShortenedUrl = _context.ShortenedUrlEntities.FirstOrDefault(p => p.LongFormUrl.Equals(createShortenedUrlRequest.LongFormUrl.AbsoluteUri) && p.RequestedPartialSlug == createShortenedUrlRequest.RequestedPartialSlug);
            if (existingShortenedUrl is not null)
            {
                if (existingShortenedUrl.ExpirationUtc is null || existingShortenedUrl.ExpirationUtc > DateTime.UtcNow)
                {
                    var existingUrl = ShortenedUrlGenerator.Get(existingShortenedUrl.RequestedPartialSlug, existingShortenedUrl.ShortenedUrlEntityId);

                    return Result.Ok(existingUrl);
                }
            }

            var shortenedUrlEntity = new ShortenedUrlEntity()
            {
                ShortenedUrlEntityId = guid,
                LongFormUrl = createShortenedUrlRequest.LongFormUrl.AbsoluteUri,
                CreatedUtc = DateTime.UtcNow,
                ExpirationUtc = expirationUtc,
                GoalCode = goalCode,
                RequestedPartialSlug = createShortenedUrlRequest.RequestedPartialSlug is null ? null : createShortenedUrlRequest.RequestedPartialSlug.ToLowerInvariant().GenerateSlug()
            };

            await _context.AddAsync(shortenedUrlEntity, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            var shortenedUrl = ShortenedUrlGenerator.Get(shortenedUrlEntity.RequestedPartialSlug, shortenedUrlEntity.ShortenedUrlEntityId);

            return Result.Ok(shortenedUrl);
        }
        catch (Exception e)
        {
            return Result.Fail("Error creating database connection");
        }
    }
}