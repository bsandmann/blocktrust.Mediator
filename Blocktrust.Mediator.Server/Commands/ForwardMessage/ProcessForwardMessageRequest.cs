namespace Blocktrust.Mediator.Server.Commands.ForwardMessage;

using DIDComm.Message.FromPriors;
using DIDComm.Message.Messages;
using ProcessMessage;

public class ProcessForwardMessageRequest : ProcessBaseRequest
{
    public ProcessForwardMessageRequest(Message unpackedMessage, string senderDid, string mediatorDid, string hostUrl, FromPrior? fromPrior) : base(unpackedMessage, senderDid, mediatorDid, hostUrl, fromPrior)
    {
    }
}