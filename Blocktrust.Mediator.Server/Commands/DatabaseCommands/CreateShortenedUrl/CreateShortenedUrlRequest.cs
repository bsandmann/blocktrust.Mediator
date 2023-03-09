namespace Blocktrust.Mediator.Server.Commands.DatabaseCommands.CreateShortenedUrl;

using Common.Models.ShortenUrl;
using FluentResults;
using MediatR;

/// <summary>
/// Request to create a new OOB invitation in the database
/// </summary>
public class CreateShortenedUrlRequest : IRequest<Result<string>>
{
    /// <summary>
    /// Request
    /// </summary>
    public CreateShortenedUrlRequest(Uri longFormUrl, string? requestedPartialSlug, EnumShortenUrlGoalCode goalCode)
    {
        LongFormUrl = longFormUrl;
        RequestedPartialSlug = requestedPartialSlug;
        GoalCode = goalCode;
    }

    public Uri LongFormUrl { get; }
    public string? RequestedPartialSlug { get; }
    public EnumShortenUrlGoalCode GoalCode { get; }

    public long? RequestValidityInSeconds { get; }
}