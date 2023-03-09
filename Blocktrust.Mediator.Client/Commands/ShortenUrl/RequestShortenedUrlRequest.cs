﻿namespace Blocktrust.Mediator.Client.Commands.ShortenUrl;

using Common.Models.ShortenUrl;
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
    /// A goal code that can be used to identify the purpose of the shortened url. Currently only support for "shorten.oobv2"
    /// </summary>
    public EnumShortenUrlGoalCode GoalCode { get; }

    /// <summary>
    /// Optional:  A string that can be used to specify the slug of the shortened url.
    /// </summary>
    public string? ShortUrlSlug { get; }


    public RequestShortenedUrlRequest(Uri mediatorEndpoint, string mediatorDid, string localDid, Uri urlToShorten, EnumShortenUrlGoalCode goalCode, long? requestValidityInSeconds = null, string? shortUrlSlug = null)
    {
        MediatorEndpoint = mediatorEndpoint;
        MediatorDid = mediatorDid;
        LocalDid = localDid;
        UrlToShorten = urlToShorten;
        RequestValidityInSeconds = requestValidityInSeconds;
        GoalCode = goalCode;
        ShortUrlSlug = shortUrlSlug;
    }
}