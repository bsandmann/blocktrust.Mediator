namespace Blocktrust.Mediator.Common.Models.ShortenUrl;

using System.Text.Json;
using FluentResults;
using ProblemReport;

public class ShortenedUrl
{
    /// <summary>
    /// Required: The url that should be shortened
    /// </summary>
    public Uri UrlToShorten { get; set; }

    /// <summary>
    /// Required: The time in seconds that the shortened url should be valid. If not provided, the url-shortener determines the validity time.
    /// </summary>
    public long RequestValidityInSeconds { get; set; }

    /// <summary>
    /// A goal code that can be used to identify the purpose of the shortened url. Currently only support for "shorten.oobv2"
    /// </summary>
    public EnumShortenUrlGoalCode GoalCode { get; set; }

    /// <summary>
    /// Optional:  A string that can be used to specify the slug of the shortened url.
    /// </summary>
    public string? ShortUrlSlug { get; set; }

    public static Result<ShortenedUrl> Parse(Dictionary<string, object?> body)
    {
        var hasUrl = body.TryGetValue("url", out var urlJson);
        string? url = null;
        if (hasUrl)
        {
            var urlJsonElement = (JsonElement)urlJson!;
            if (urlJsonElement.ValueKind is JsonValueKind.String)
            {
                url = urlJsonElement.GetString();
            }
        }

        if (!hasUrl || string.IsNullOrWhiteSpace(url))
        {
            return Result.Fail("Invalid body format: invalid_url");
        }
        
        var isParsed = Uri.TryCreate(url, UriKind.Absolute, out var urlUri);
        if (!isParsed || urlUri is null)
        {
            return Result.Fail("Invalid body format: invalid_url");
        }

        long requestedValiditySeconds = 0;
        var hasRequestedValiditySeconds = body.TryGetValue("requested_validity_seconds", out var requestedValiditySecondsJson);
        if (hasRequestedValiditySeconds) 
        {
            var requestedValiditySecondsJsonElement = (JsonElement)requestedValiditySecondsJson!;
            if (requestedValiditySecondsJsonElement.ValueKind is JsonValueKind.Number)
            {
                var parsedSeconds = requestedValiditySecondsJsonElement.GetInt64();
                if (parsedSeconds >= 0)
                {
                   requestedValiditySeconds = parsedSeconds; 
                }
                else
                {
                    return Result.Fail("Invalid value for requested_validity_seconds. Must be a positive integer");
                }
            }
        }

        if (!hasRequestedValiditySeconds)
        {
            requestedValiditySeconds = 0;
        }
        else if (requestedValiditySeconds < 0 || requestedValiditySeconds > 315360000) //ten years
        {
            return Result.Fail("Invalid body format: invalid requested_validity_seconds");
        }

        var hasGoalCode = body.TryGetValue("url", out var goalCodeJson);
        string? goalCode = null;
        if (hasGoalCode)
        {
            var goalCodeJsonElement = (JsonElement)goalCodeJson!;
            if (goalCodeJsonElement.ValueKind is JsonValueKind.String)
            {
                goalCode = goalCodeJsonElement.GetString();
            }
        }

        if (!hasGoalCode || goalCode.Equals("shorten.oobv2", StringComparison.InvariantCultureIgnoreCase))
        {
            return Result.Fail("Invalid body format: invalid_goal_code. Only supported goal_code is 'shorten.oobv2'");
        }


        var hasShortUrlSlug = body.TryGetValue("short_url_slug", out var shortUrlSlugJson);
        string? shortUrlSlug = null;
        if (hasShortUrlSlug)
        {
            var shortUrlSlugJsonElement = (JsonElement)shortUrlSlugJson!;
            if (shortUrlSlugJsonElement.ValueKind is JsonValueKind.String)
            {
                shortUrlSlug = shortUrlSlugJsonElement.GetString();
            }
        }

        return Result.Ok(new ShortenedUrl()
        {
            UrlToShorten = urlUri,
            RequestValidityInSeconds = requestedValiditySeconds,
            GoalCode = EnumShortenUrlGoalCode.ShortenOOBv2,
            ShortUrlSlug = shortUrlSlug
        });
    }
}