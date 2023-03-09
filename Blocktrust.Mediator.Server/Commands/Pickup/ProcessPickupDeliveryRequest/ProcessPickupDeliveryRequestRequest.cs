namespace Blocktrust.Mediator.Server.Commands.Pickup.ProcessPickupDeliveryRequest;

using Blocktrust.DIDComm.Message.FromPriors;
using Blocktrust.DIDComm.Message.Messages;
using MediatR;

public class ProcessPickupDeliveryRequestRequest: IRequest<Message>
{
    public Message UnpackedMessage { get; }
    public string SenderDid { get; }
    public string MediatorDid { get; }
    public string HostUrl { get; }
    public FromPrior? FromPrior { get; }
    
    public ProcessPickupDeliveryRequestRequest(Message unpackedMessage, string senderDid, string mediatorDid, string hostUrl, FromPrior? fromPrior)
    {
        UnpackedMessage = unpackedMessage;
        SenderDid = senderDid;
        MediatorDid = mediatorDid;
        HostUrl = hostUrl;
        FromPrior = fromPrior;
    }
}