namespace Blocktrust.Mediator.Server.Commands.Pickup.ProcessStatusRequest;

using DIDComm.Message.FromPriors;
using DIDComm.Message.Messages;
using FluentResults;
using MediatR;

public class ProcessPickupStatusRequestRequest: IRequest<Result<Message>>
{
    public Message UnpackedMessage { get; }
    public string SenderDid { get; }
    public string MediatorDid { get; }
    public string HostUrl { get; }
    public FromPrior? FromPrior { get; }
    
    public ProcessPickupStatusRequestRequest(Message unpackedMessage, string senderDid, string mediatorDid, string hostUrl, FromPrior? fromPrior)
    {
        UnpackedMessage = unpackedMessage;
        SenderDid = senderDid;
        MediatorDid = mediatorDid;
        HostUrl = hostUrl;
        FromPrior = fromPrior;
    }
}