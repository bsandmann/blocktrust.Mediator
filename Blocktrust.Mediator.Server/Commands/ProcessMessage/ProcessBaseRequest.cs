namespace Blocktrust.Mediator.Server.Commands.ProcessMessage;

using DIDComm.Message.FromPriors;
using DIDComm.Message.Messages;
using MediatR;

/// <summary>
/// Request to Process DIDComm messages for specific message types
/// </summary>
public abstract class ProcessBaseRequest : IRequest<Message?>
{
    public Message UnpackedMessage { get; }
    public string SenderDid { get; }
    public string MediatorDid { get; }
    public string HostUrl { get; }
    public FromPrior? FromPrior { get; }

    public ProcessBaseRequest(Message unpackedMessage, string senderDid, string mediatorDid, string hostUrl, FromPrior? fromPrior)
    {
        UnpackedMessage = unpackedMessage;
        SenderDid = senderDid;
        MediatorDid = mediatorDid;
        HostUrl = hostUrl;
        FromPrior = fromPrior;
    } 
}