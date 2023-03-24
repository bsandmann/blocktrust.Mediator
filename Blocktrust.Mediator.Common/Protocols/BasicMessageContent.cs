namespace Blocktrust.Mediator.Common.Protocols;

public class BasicMessageContent
{
    public string Message { get; set; }
    public string MessageId { get; set; }
    public string From { get; set; }
    public List<string> Tos { get; set; }
    
    public DateTime CreatedUtc { get; set; }

    public BasicMessageContent(string message, string messageId, string from, List<string> tos, DateTime createdUtc)
    {
        Message = message;
        MessageId = messageId;
        From = from;
        Tos = tos;
        CreatedUtc = createdUtc;
    }

}