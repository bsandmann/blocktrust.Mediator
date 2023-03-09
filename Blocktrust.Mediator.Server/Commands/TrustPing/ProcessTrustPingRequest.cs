namespace Blocktrust.Mediator.Server.Commands.TrustPing;

using Blocktrust.DIDComm.Message.FromPriors;
using Blocktrust.DIDComm.Message.Messages;
using MediatR;

//TODO unify all these requests!
public class ProcessTrustPingRequest : IRequest<Message?>
{
    public Message UnpackedMessage { get; }
    public string SenderDid { get; }
    public string MediatorDid { get; }
    public string HostUrl { get; }
    public FromPrior? FromPrior { get; }

    public ProcessTrustPingRequest(Message unpackedMessage, string senderDid, string mediatorDid, string hostUrl, FromPrior? fromPrior)
    {
        UnpackedMessage = unpackedMessage;
        SenderDid = senderDid;
        MediatorDid = mediatorDid;
        HostUrl = hostUrl;
        FromPrior = fromPrior;
    }
}