namespace Blocktrust.Mediator.Common.Models.Pickup;

using DIDComm.Message.Messages;

public class DeliveryResponseModel
{
    public string? MessageId { get; set; }
    public Message? Message { get; set; }
    public bool IsSuccess { get; set; }
    public string? Error { get; set; }

    public DeliveryResponseModel(string error)
    {
       this.IsSuccess = false;
       this.MessageId = null;
       this.Error = error;
       this.Message = null;
    }
    
    public DeliveryResponseModel(string messageId, Message message)
    {
       this.IsSuccess = true;
       this.MessageId = messageId;
       this.Message = message;
       this.Error = null;
    }
}