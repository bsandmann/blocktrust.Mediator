namespace Blocktrust.Mediator.Server.Commands.MediatorCoordinator.ProcessMediationRequest;

using Blocktrust.DIDComm.Message.FromPriors;
using Blocktrust.DIDComm.Message.Messages;
using ProcessMessage;

public class ProcessMediationRequestRequest  : ProcessBaseRequest
{
    public ProcessMediationRequestRequest(Message unpackedMessage, string senderDid, string mediatorDid, string hostUrl, FromPrior? fromPrior) : base(unpackedMessage, senderDid, mediatorDid, hostUrl, fromPrior)
    {
    }
}