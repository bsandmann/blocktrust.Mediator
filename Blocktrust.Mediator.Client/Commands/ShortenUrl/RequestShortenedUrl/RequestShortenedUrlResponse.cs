namespace Blocktrust.Mediator.Client.Commands.ShortenUrl.RequestShortenedUrl;

using System.Text.Json;
using Common.Models.ProblemReport;
using FluentResults;

public class RequestShortenedUrlResponse
{
    public Uri? ShortenedUrl { get; }
    public DateTime? ExpiresTimeUtc { get; }

    public ProblemReport? ProblemReport { get; }

    public RequestShortenedUrlResponse(Uri shortenedUrl, DateTime? expiresTimeUtc = null)
    {
        ShortenedUrl = shortenedUrl;
        ExpiresTimeUtc = expiresTimeUtc;
    }

    public RequestShortenedUrlResponse(ProblemReport problemReport)
    {
        ProblemReport = problemReport;
    }


    public static Result<RequestShortenedUrlResponse> Parse(Dictionary<string, object> body)
    {
        string? shortenedUrl = null;
        DateTime? expiresTime = null;
        if (body.ContainsKey("shortened_url"))
        {
            body.TryGetValue("shortened_url", out var shortenedUrlJson);
            var shortenedUrlJsonElement = (JsonElement)shortenedUrlJson;
            if (shortenedUrlJsonElement.ValueKind != JsonValueKind.String)
            {
                return Result.Fail("Error: Malformed 'shortened_url' in body");
            }

            shortenedUrl = shortenedUrlJsonElement.GetString();
        }
        else
        {
            return Result.Fail("Error: Missing 'shortened_url' in body");
        }

        if (body.ContainsKey("expires_time"))
        {
            body.TryGetValue("expires_time", out var expiresTimesJson);
            var expiresTimesJsonElement = (JsonElement)expiresTimesJson;
            if (expiresTimesJsonElement.ValueKind == JsonValueKind.Number)
            {
                expiresTime = DateTimeOffset.FromUnixTimeSeconds(expiresTimesJsonElement.GetInt64()).DateTime;
            }
            else if (expiresTimesJsonElement.ValueKind == JsonValueKind.String)
            {
                var longValue = long.Parse(expiresTimesJsonElement.GetString()!);
                expiresTime = DateTimeOffset.FromUnixTimeSeconds(longValue).DateTime;
            }
            else
            {
                return Result.Fail($"Error: Malformed 'expires_time' in body: '{expiresTimesJsonElement.ToString()}'");
            }
        }

        return Result.Ok(new RequestShortenedUrlResponse(new Uri(shortenedUrl)!, expiresTime));
    }
}