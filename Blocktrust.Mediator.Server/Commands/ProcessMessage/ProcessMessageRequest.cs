namespace Blocktrust.Mediator.Server.Commands.ProcessMessage;

using Blocktrust.DIDComm.Model.UnpackResultModels;
using MediatR;

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