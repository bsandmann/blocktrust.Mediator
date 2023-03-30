namespace Blocktrust.Mediator.Server.Commands.TrustPing;

using Blocktrust.DIDComm.Message.Messages;
using ProcessMessage;

/// <inheritdoc />
public class ProcessTrustPingRequest : ProcessBaseRequest
{
    /// <inheritdoc />
    public ProcessTrustPingRequest(Message unpackedMessage) : base(unpackedMessage)
    {
    }
}