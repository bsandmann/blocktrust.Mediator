namespace Blocktrust.Mediator.Server.Commands.Pickup.ProcessPickupLiveDeliveryChange;

using Blocktrust.DIDComm.Message.FromPriors;
using Blocktrust.DIDComm.Message.Messages;
using ProcessMessage;

public class ProcessPickupDeliveryChangeRequest : ProcessBaseRequest
{
    public ProcessPickupDeliveryChangeRequest(Message unpackedMessage, string senderDid, string mediatorDid, string hostUrl, FromPrior? fromPrior) : base(unpackedMessage, senderDid, mediatorDid, hostUrl, fromPrior)
    {
    }
}