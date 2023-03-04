namespace Blocktrust.Mediator.Server.Commands.MediatorCoordinator.ProcessUpdateMediatorKeys;

using DIDComm.Message.FromPriors;
using DIDComm.Message.Messages;
using FluentResults;
using MediatR;

public class ProcessUpdateMediatorKeysRequest: IRequest<Result<Message>>
{
    public Message UnpackedMessage { get; }
    public string SenderDid { get; }
    public string MediatorDid { get; }
    public string HostUrl { get; }
    public FromPrior? FromPrior { get; }
    
    public ProcessUpdateMediatorKeysRequest(Message unpackedMessage, string senderDid, string mediatorDid, string hostUrl, FromPrior? fromPrior)
    {
        UnpackedMessage = unpackedMessage;
        SenderDid = senderDid;
        MediatorDid = mediatorDid;
        HostUrl = hostUrl;
        FromPrior = fromPrior;
    }
}