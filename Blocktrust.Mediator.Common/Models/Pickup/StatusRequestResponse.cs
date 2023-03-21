namespace Blocktrust.Mediator.Common.Models.Pickup;

using System.Text.Json;
using System.Text.Json.Serialization;
using FluentResults;
using ProblemReport;

public class StatusRequestResponse
{
    /// <summary>
    /// Required: Number of messages in the mediator ready to be picked up
    /// </summary>
    [JsonPropertyName("message_count")]
    public int MessageCount { get; set; }

    /// <summary>
    /// Optional: When present, this status response is only related to messages for that specific recipient
    /// </summary>
    [JsonPropertyName("recipient_did")]
    public string? RecipientDid { get; set; }

    /// <summary>
    /// Optional: The longest waited time in seconds for a message to be picked up
    /// </summary>
    [JsonPropertyName("longest_waited_seconds")]
    public long? LongestWaitedSeconds { get; set; }

    /// <summary>
    /// Optional: The timestamp of the newest message
    /// </summary>
    [JsonPropertyName("newest_received_time")]
    public long? NewestMessageTime { get; set; }

    /// <summary>
    /// Optional: The timestamp of the oldest message
    /// </summary>
    [JsonPropertyName("oldest_received_time")]
    public long? OldestMessageTime { get; set; }

    /// <summary>
    /// Optional: Total size of all messages waiting to be picked up
    /// </summary>
    [JsonPropertyName("total_bytes")]
    public long? TotalByteSize { get; set; }

    /// <summary>
    /// Optional: Flag if the live-delivery mode ist supported 
    /// </summary>
    [JsonPropertyName("live_delivery")]
    public bool? LiveDelivery { get; set; }

    public ProblemReport? ProblemReport { get; set; } 
    
    public StatusRequestResponse()
    {
    }
    
    public StatusRequestResponse(ProblemReport problemReport)
    {
        this.ProblemReport = problemReport;
    }

    public static Result<StatusRequestResponse> Parse(Dictionary<string, object?> body)
    {
        var statusRequestResponse = new StatusRequestResponse();
        if (body.ContainsKey("message_count"))
        {
            body.TryGetValue("message_count", out var messageCount);
            var messageCountJsonElement = (JsonElement)messageCount!;
            if (messageCountJsonElement.ValueKind is JsonValueKind.Number)
            {
                statusRequestResponse.MessageCount = messageCountJsonElement.GetInt32();
            }
        }
        else
        {
            return Result.Fail("Required content: message_count is missing in the body");
        }

        if (body.ContainsKey("recipient_did"))
        {
            body.TryGetValue("recipient_did", out var recipientDid);
            var recipientDidJsonElement = (JsonElement)recipientDid!;
            if (recipientDidJsonElement.ValueKind is JsonValueKind.String)
            {
                statusRequestResponse.RecipientDid = recipientDidJsonElement.GetString();
            }
        }

        if (body.ContainsKey("longest_waited_seconds"))
        {
            body.TryGetValue("longest_waited_seconds", out var longestWaitedSeconds);
            var longestWaitedSecondsJsonElement = (JsonElement)longestWaitedSeconds!;
            if (longestWaitedSecondsJsonElement.ValueKind is JsonValueKind.Number)
            {
                statusRequestResponse.LongestWaitedSeconds = longestWaitedSecondsJsonElement.GetInt64();
            }
        }

        if (body.ContainsKey("newest_received_time"))
        {
            body.TryGetValue("newest_received_time", out var newestMessageTime);
            var newestMessageTimeJsonElement = (JsonElement)newestMessageTime!;
            if (newestMessageTimeJsonElement.ValueKind is JsonValueKind.Number)
            {
                statusRequestResponse.LongestWaitedSeconds = newestMessageTimeJsonElement.GetInt64();
            }
        }

        if (body.ContainsKey("oldest_received_time"))
        {
            body.TryGetValue("oldest_received_time", out var oldestMessageTime);
            var oldestMessageTimeJsonElement = (JsonElement)oldestMessageTime!;
            if (oldestMessageTimeJsonElement.ValueKind is JsonValueKind.Number)
            {
                statusRequestResponse.LongestWaitedSeconds = oldestMessageTimeJsonElement.GetInt64();
            }
        }

        if (body.ContainsKey("total_bytes"))
        {
            body.TryGetValue("total_bytes", out var totalByteSize);
            var totalByteSizeJsonElement = (JsonElement)totalByteSize!;
            if (totalByteSizeJsonElement.ValueKind is JsonValueKind.Number)
            {
                statusRequestResponse.LongestWaitedSeconds = totalByteSizeJsonElement.GetInt64();
            }
        }

        if (body.ContainsKey("live_delivery"))
        {
            body.TryGetValue("live_delivery", out var liveDelivery);
            var liveDeliveryJsonElement = (JsonElement)liveDelivery!;
            if (liveDeliveryJsonElement.ValueKind is JsonValueKind.True || liveDeliveryJsonElement.ValueKind is JsonValueKind.False)
            {
                statusRequestResponse.LiveDelivery = liveDeliveryJsonElement.GetBoolean();
            }
        } 
        
        return Result.Ok(statusRequestResponse);
    }
}