namespace Blocktrust.Mediator.Server.Commands.ForwardMessage;

using DIDComm.Message.Messages;
using ProcessMessage;

/// <inheritdoc />
public class ProcessForwardMessageRequest : ProcessBaseRequest
{
    /// <inheritdoc />
    public ProcessForwardMessageRequest(Message unpackedMessage, string? mediatorDid) : base(unpackedMessage, mediatorDid)
    {
    }
}