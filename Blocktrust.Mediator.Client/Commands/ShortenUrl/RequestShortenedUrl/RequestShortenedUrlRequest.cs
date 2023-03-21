namespace Blocktrust.Mediator.Client.Commands.ShortenUrl.RequestShortenedUrl;

using Blocktrust.Mediator.Common.Models.ShortenUrl;
using FluentResults;
using MediatR;

public class RequestShortenedUrlRequest : IRequest<Result<RequestShortenedUrlResponse>>
{
    public Uri MediatorEndpoint { get; }
    public string MediatorDid { get; }
    public string LocalDid { get; }

    /// <summary>
    /// Required: The url that should be shortened
    /// </summary>
    public Uri UrlToShorten { get; }

    /// <summary>
    /// Required: The time in seconds that the shortened url should be valid. If not provided, the url-shortener determines the validity time.
    /// </summary>
    public long? RequestValidityInSeconds { get; }

    /// <summary>
    /// Optional:  A string that can be used to specify the slug of the shortened url.
    /// </summary>
    public string? ShortUrlSlug { get; }


    public RequestShortenedUrlRequest(Uri mediatorEndpoint, string mediatorDid, string localDid, Uri urlToShorten, long? requestValidityInSeconds = null, string? shortUrlSlug = null)
    {
        MediatorEndpoint = mediatorEndpoint;
        MediatorDid = mediatorDid;
        LocalDid = localDid;
        UrlToShorten = urlToShorten;
        RequestValidityInSeconds = requestValidityInSeconds;
        ShortUrlSlug = shortUrlSlug;
    }
}