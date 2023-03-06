﻿namespace Blocktrust.Mediator.Server.Models;

public class MessagesStatusModel
{
    /// <summary>
    /// Required: Number of messages in the mediator ready to be picked up
    /// </summary>
    public int MessageCount { get; set; }

    /// <summary>
    /// Optional: When present, this status response is only related to messages for that specific recipient
    /// </summary>
    public string? RecipientDid { get; set; }

    /// <summary>
    /// Optional: The longest waited time in seconds for a message to be picked up
    /// </summary>
    public long? LongestWaitedSeconds { get; set; }

    /// <summary>
    /// Optional: The timestamp of the newest message
    /// </summary>
    public long? NewestMessageTime { get; set; }

    /// <summary>
    /// Optional: The timestamp of the oldest message
    /// </summary>
    public long? OldestMessageTime { get; set; }

    /// <summary>
    /// Optional: Total size of all messages waiting to be picked up
    /// </summary>
    public long? TotalByteSize { get; set; }

    public MessagesStatusModel(int messageCount, string? recipientDid, long? longestWaitedSeconds, long? newestMessageTime, long? oldestMessageTime, long? totalByteSize)
    {
        MessageCount = messageCount;
        RecipientDid = recipientDid;
        LongestWaitedSeconds = longestWaitedSeconds;
        NewestMessageTime = newestMessageTime;
        OldestMessageTime = oldestMessageTime;
        TotalByteSize = totalByteSize;
    }
}