namespace Blocktrust.Mediator.Server.Commands.ShortenUrl;

using Blocktrust.DIDComm.Message.FromPriors;
using Blocktrust.DIDComm.Message.Messages;
using Blocktrust.Mediator.Server.Commands.ProcessMessage;

/// <inheritdoc />
public class ProcessShortenUrlRequest  : ProcessBaseRequest
{
    /// <inheritdoc />
    public ProcessShortenUrlRequest(Message unpackedMessage, string senderDid, string mediatorDid, string hostUrl, FromPrior? fromPrior) : base(unpackedMessage, senderDid, mediatorDid, hostUrl, fromPrior)
    {
    }
}