namespace Blocktrust.Mediator.Client.Commands.Pickup.DeliveryRequest;

using Blocktrust.Mediator.Common.Models.Pickup;

public class DeliveryRequestResponse
{
    /// <summary>
    /// Flag if we have messages to be picked up
    /// </summary>
    public bool HasMessages { get; }

    /// <summary>
    /// If there are messages, this list contains these
    /// </summary>
    public List<DeliveryResponseModel>? Messages { get; }

    /// <summary>
    /// The Status is only used, when there are no messages to be picked up
    /// </summary>
    public StatusRequestResponse? Status { get; }


    /// <summary>
    /// Constructor for the case when there are no messages to be picked up
    /// </summary>
    /// <param name="status"></param>
    public DeliveryRequestResponse(StatusRequestResponse status)
    {
        this.Status = status;
        this.Messages = null;
        this.HasMessages = false;
    }
    
    /// <summary>
    /// Constructor for the case when there are no messages to be picked up
    /// </summary>
    /// <param name="messages"></param>
    public DeliveryRequestResponse(List<DeliveryResponseModel> messages)
    {
        this.Status = null;
        this.Messages = messages;
        this.HasMessages = true;
    }
}