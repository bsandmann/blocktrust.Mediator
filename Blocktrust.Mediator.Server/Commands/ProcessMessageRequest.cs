namespace Blocktrust.Mediator.Server.Commands;

using DIDComm.Message.Messages;
using DIDComm.Model.UnpackResultModels;
using FluentResults;
using MediatR;
using ProcessMessage;

public class ProcessMessageRequest : IRequest<ProcessMessageResponse>
{
    public string SenderOldDid { get; }
    public string SenderDid { get; }
    public string HostUrl { get; }
    public UnpackResult UnpackResult { get; }


    public ProcessMessageRequest(string senderOldDid, string senderDid, string hostUrl, UnpackResult unpackResult)
    {
        SenderOldDid = senderOldDid;
        SenderDid = senderDid;
        HostUrl = hostUrl;
        UnpackResult = unpackResult;
    }
}