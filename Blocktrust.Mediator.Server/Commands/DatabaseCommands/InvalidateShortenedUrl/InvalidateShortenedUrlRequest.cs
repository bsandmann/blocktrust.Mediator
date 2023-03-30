namespace Blocktrust.Mediator.Server.Commands.DatabaseCommands.InvalidateShortenedUrl;

using FluentResults;
using MediatR;

/// <summary>
/// Request to delete a shortened Url entry in the database 
/// </summary>
public class InvalidateShortenedUrlRequest : IRequest<Result>
{
    /// <summary>
    /// Request
    /// </summary>
    public InvalidateShortenedUrlRequest(Guid shortenedUrlEntityId)
    {
        ShortenedUrlEntityId = shortenedUrlEntityId;
    }

    public Guid ShortenedUrlEntityId { get; set; }
}