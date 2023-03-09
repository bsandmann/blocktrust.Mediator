namespace Blocktrust.Mediator.Server.Commands.MediatorCoordinator.ProcessQueryMediatorKeys;

using DIDComm.Message.FromPriors;
using DIDComm.Message.Messages;
using ProcessMessage;

public class ProcessQueryMediatorKeysRequest  : ProcessBaseRequest
{
    public ProcessQueryMediatorKeysRequest(Message unpackedMessage, string senderDid, string mediatorDid, string hostUrl, FromPrior? fromPrior) : base(unpackedMessage, senderDid, mediatorDid, hostUrl, fromPrior)
    {
    }
}