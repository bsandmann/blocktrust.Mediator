namespace Blocktrust.Mediator.Server.Commands.Pickup.ProcessPickupDeliveryRequest;

using Blocktrust.DIDComm.Message.FromPriors;
using Blocktrust.DIDComm.Message.Messages;
using ProcessMessage;

/// <inheritdoc />
public class ProcessPickupDeliveryRequestRequest : ProcessBaseRequest
{
    /// <inheritdoc />
    public ProcessPickupDeliveryRequestRequest(Message unpackedMessage, string? senderDid, string? mediatorDid, string hostUrl, FromPrior? fromPrior) : base(unpackedMessage, senderDid, mediatorDid, hostUrl, fromPrior)
    {
    }
}