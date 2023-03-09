namespace Blocktrust.Mediator.Server.Commands.DiscoverFeatures;

using Blocktrust.DIDComm.Message.FromPriors;
using Blocktrust.DIDComm.Message.Messages;
using ProcessMessage;

public class ProcessDiscoverFeaturesRequest  : ProcessBaseRequest
{
    public ProcessDiscoverFeaturesRequest(Message unpackedMessage, string senderDid, string mediatorDid, string hostUrl, FromPrior? fromPrior) : base(unpackedMessage, senderDid, mediatorDid, hostUrl, fromPrior)
    {
    }
}