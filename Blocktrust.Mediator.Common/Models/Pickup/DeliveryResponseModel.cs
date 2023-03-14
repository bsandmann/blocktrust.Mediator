namespace Blocktrust.Mediator.Common.Models.Pickup;

using DIDComm.Message.Messages;
using DIDComm.Model.UnpackResultModels;

public class DeliveryResponseModel
{
    public string? MessageId { get; }
    public Message? Message { get; }
    public Metadata? Metadata { get; }
    public bool IsSuccess { get; }
    public string? Error { get; }

    public DeliveryResponseModel(string error)
    {
       this.IsSuccess = false;
       this.MessageId = null;
       this.Error = error;
       this.Message = null;
       this.Metadata = null;
    }
    
    public DeliveryResponseModel(string messageId, Message message, Metadata metadata)
    {
       this.IsSuccess = true;
       this.MessageId = messageId;
       this.Message = message;
       this.Metadata = metadata;
       this.Error = null;
    }
}