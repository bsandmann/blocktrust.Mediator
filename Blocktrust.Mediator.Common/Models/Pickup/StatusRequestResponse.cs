namespace Blocktrust.Mediator.Common.Models.Pickup;

using System.Text.Json.Serialization;

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
    
}