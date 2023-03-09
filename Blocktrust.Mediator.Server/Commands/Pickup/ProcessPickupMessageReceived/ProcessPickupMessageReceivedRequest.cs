namespace Blocktrust.Mediator.Server.Commands.Pickup.ProcessPickupMessageReceived;

using Blocktrust.DIDComm.Message.FromPriors;
using Blocktrust.DIDComm.Message.Messages;
using ProcessMessage;

public class ProcessPickupMessageReceivedRequest : ProcessBaseRequest
{
    public ProcessPickupMessageReceivedRequest(Message unpackedMessage, string senderDid, string mediatorDid, string hostUrl, FromPrior? fromPrior) : base(unpackedMessage, senderDid, mediatorDid, hostUrl, fromPrior)
    {
    }
}