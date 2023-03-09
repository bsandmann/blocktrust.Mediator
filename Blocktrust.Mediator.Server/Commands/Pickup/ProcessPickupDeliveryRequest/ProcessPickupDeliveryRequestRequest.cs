namespace Blocktrust.Mediator.Server.Commands.Pickup.ProcessPickupDeliveryRequest;

using Blocktrust.DIDComm.Message.FromPriors;
using Blocktrust.DIDComm.Message.Messages;
using ProcessMessage;

public class ProcessPickupDeliveryRequestRequest : ProcessBaseRequest
{
    public ProcessPickupDeliveryRequestRequest(Message unpackedMessage, string senderDid, string mediatorDid, string hostUrl, FromPrior? fromPrior) : base(unpackedMessage, senderDid, mediatorDid, hostUrl, fromPrior)
    {
    }
}