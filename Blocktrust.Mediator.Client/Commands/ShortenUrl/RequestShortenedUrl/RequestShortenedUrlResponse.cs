namespace Blocktrust.Mediator.Client.Commands.ShortenUrl.RequestShortenedUrl;

using System.Text.Json;
using FluentResults;

public class RequestShortenedUrlResponse
{
    public Uri ShortenedUrl { get; set; }
    public DateTime? ExpiresTimeUtc { get; set; }

    public RequestShortenedUrlResponse(Uri shortenedUrl, DateTime? expiresTimeUtc = null)
    {
        ShortenedUrl = shortenedUrl;
        ExpiresTimeUtc = expiresTimeUtc;
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
            if (expiresTimesJsonElement.ValueKind != JsonValueKind.Number)
            {
                return Result.Fail("Error: Malformed 'expires_time' in body");
            }

            expiresTime = DateTimeOffset.FromUnixTimeSeconds(expiresTimesJsonElement.GetInt64()).DateTime;
        }

        return Result.Ok(new RequestShortenedUrlResponse(new Uri(shortenedUrl)!, expiresTime));
    }
}