﻿namespace Blocktrust.Mediator.Client.Commands.ShortenUrl.InvalidateShortenedUrl;

using Common.Models.ProblemReport;
using FluentResults;
using MediatR;

public class InvalidateShortenedUrlRequest : IRequest<Result<ProblemReport>>
{
    public Uri MediatorEndpoint { get; }
    public string MediatorDid { get; }
    public string LocalDid { get; }

    /// <summary>
    /// Required: The shortened url which should now be invalidated
    /// </summary>
    public Uri ShortenedUrl { get; }



    public InvalidateShortenedUrlRequest(Uri mediatorEndpoint, string mediatorDid, string localDid, Uri shortenedUrl)
    {
        MediatorEndpoint = mediatorEndpoint;
        MediatorDid = mediatorDid;
        LocalDid = localDid;
        ShortenedUrl = shortenedUrl;
    }
}