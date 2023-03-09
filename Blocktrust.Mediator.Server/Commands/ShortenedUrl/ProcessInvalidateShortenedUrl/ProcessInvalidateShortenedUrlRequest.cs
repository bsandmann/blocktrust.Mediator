namespace Blocktrust.Mediator.Server.Commands.ShortenedUrl.ProcessInvalidateShortenedUrl;

using Blocktrust.DIDComm.Message.FromPriors;
using Blocktrust.DIDComm.Message.Messages;
using Blocktrust.Mediator.Server.Commands.ProcessMessage;

/// <inheritdoc />
public class ProcessInvalidateShortenedUrlRequest  : ProcessBaseRequest
{
    /// <inheritdoc />
    public ProcessInvalidateShortenedUrlRequest(Message unpackedMessage, string senderDid, string mediatorDid, string hostUrl, FromPrior? fromPrior) : base(unpackedMessage, senderDid, mediatorDid, hostUrl, fromPrior)
    {
    }
}