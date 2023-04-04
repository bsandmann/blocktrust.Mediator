namespace Blocktrust.Mediator.Server.Commands.BasicMessageAB;

using Blocktrust.DIDComm.Message.Messages;
using Blocktrust.Mediator.Server.Commands.ProcessMessage;
using DIDComm.Message.FromPriors;

/// <inheritdoc />
public class BasicMessageAbRequest : ProcessBaseRequest
{
    /// <inheritdoc />
    public BasicMessageAbRequest(Message unpackedMessage, string? senderDid, string? mediatorDid, string hostUrl, FromPrior? fromPrior) : base(unpackedMessage, senderDid, mediatorDid, hostUrl, fromPrior)
    {
    }
}