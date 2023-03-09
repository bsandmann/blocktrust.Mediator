namespace Blocktrust.Mediator.Server.Commands.MediatorCoordinator.ProcessUpdateMediatorKeys;

using DIDComm.Message.FromPriors;
using DIDComm.Message.Messages;
using ProcessMessage;

/// <inheritdoc />
public class ProcessUpdateMediatorKeysRequest : ProcessBaseRequest
{
    /// <inheritdoc />
    public ProcessUpdateMediatorKeysRequest(Message unpackedMessage, string senderDid, string mediatorDid, string hostUrl, FromPrior? fromPrior) : base(unpackedMessage, senderDid, mediatorDid, hostUrl, fromPrior)
    {
    }
}