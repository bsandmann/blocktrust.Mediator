namespace Blocktrust.Mediator.Server.Models;

public class StoredMessageModel
{
    public StoredMessageModel(string messageId, string message)
    {
        MessageId = messageId;
        Message = message;
    }

    public string MessageId { get; set; }
    public string Message { get; set; }
    //TODO we need the type of the message. Currently we assume it is a JSON string
    //Also other metadata ??
}