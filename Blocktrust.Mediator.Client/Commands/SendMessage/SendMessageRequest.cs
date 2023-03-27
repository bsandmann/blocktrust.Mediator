namespace Blocktrust.Mediator.Client.Commands.SendMessage;

using DIDComm.Message.Messages;
using FluentResults;
using MediatR;

public class SendMessageRequest: IRequest<Result<Message>>
{
    public Uri RemoteEndpoint { get; }
    public string RemoteDid { get; }
    public string LocalDid { get; }
    public Message Message {get;}

    public SendMessageRequest(Uri remoteEndpoint, string remoteDid, string localDid, Message message)
    {
        RemoteEndpoint = remoteEndpoint;
        RemoteDid = remoteDid;
        LocalDid = localDid;
        Message = message;
    }
}