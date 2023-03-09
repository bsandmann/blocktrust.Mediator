namespace Blocktrust.Mediator.Server.Commands.DatabaseCommands.GetShortenedUrl;

using FluentResults;
using MediatR;

/// <summary>
/// Request to resolve a shortend URL 
/// </summary>
public class GetShortenedUrlRequest : IRequest<Result<string>>
{
    /// <summary>
    /// Request
    /// </summary>
    public GetShortenedUrlRequest(Guid shortenedUrlEntityId)
    {
        ShortenedUrlEntityId = shortenedUrlEntityId;
    }

    public Guid ShortenedUrlEntityId { get; }
}