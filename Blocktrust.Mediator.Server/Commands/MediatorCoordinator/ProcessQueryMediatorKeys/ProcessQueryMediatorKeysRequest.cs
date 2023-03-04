namespace Blocktrust.Mediator.Server.Commands.MediatorCoordinator.ProcessQueryMediatorKeys;

using DIDComm.Message.FromPriors;
using DIDComm.Message.Messages;
using FluentResults;
using MediatR;

public class ProcessQueryMediatorKeysRequest : IRequest<Result<Message>>
{
    public Message UnpackedMessage { get; set; }
    public string SenderDid { get; set; }
    public string MediatorDid { get; set; }
    public string HostUrl { get; set; }
    public FromPrior? FromPrior { get; set; }
    
    public ProcessQueryMediatorKeysRequest(Message unpackedMessage, string senderDid, string mediatorDid, string hostUrl, FromPrior? fromPrior)
    {
        UnpackedMessage = unpackedMessage;
        SenderDid = senderDid;
        MediatorDid = mediatorDid;
        HostUrl = hostUrl;
        FromPrior = fromPrior;
    }
}