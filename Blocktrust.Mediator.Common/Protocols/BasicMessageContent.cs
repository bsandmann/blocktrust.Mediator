namespace Blocktrust.Mediator.Common.Protocols;

public class BasicMessageContent
{
    public string Message { get; set; }
    public string MessageId { get; set; }
    public string From { get; set; }
    public List<string> Tos { get; set; }

    public BasicMessageContent(string message, string messageId, string from, List<string> tos)
    {
        Message = message;
        MessageId = messageId;
        From = from;
        Tos = tos;
    }

}