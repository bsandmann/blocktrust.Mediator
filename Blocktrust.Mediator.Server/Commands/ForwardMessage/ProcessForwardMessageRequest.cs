namespace Blocktrust.Mediator.Server.Commands.ForwardMessage;

using DIDComm.Message.FromPriors;
using DIDComm.Message.Messages;
using ProcessMessage;

/// <inheritdoc />
public class ProcessForwardMessageRequest : ProcessBaseRequest
{
    /// <inheritdoc />
    public ProcessForwardMessageRequest(Message unpackedMessage, string senderDid, string mediatorDid, string hostUrl, FromPrior? fromPrior) : base(unpackedMessage, senderDid, mediatorDid, hostUrl, fromPrior)
    {
    }
}