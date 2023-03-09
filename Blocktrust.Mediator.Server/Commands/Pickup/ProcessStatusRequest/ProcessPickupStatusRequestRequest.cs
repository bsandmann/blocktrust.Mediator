namespace Blocktrust.Mediator.Server.Commands.Pickup.ProcessStatusRequest;

using DIDComm.Message.FromPriors;
using DIDComm.Message.Messages;
using ProcessMessage;

/// <inheritdoc />
public class ProcessPickupStatusRequestRequest : ProcessBaseRequest
{
    /// <inheritdoc />
    public ProcessPickupStatusRequestRequest(Message unpackedMessage, string senderDid, string mediatorDid, string hostUrl, FromPrior? fromPrior) : base(unpackedMessage, senderDid, mediatorDid, hostUrl, fromPrior)
    {
    }
}