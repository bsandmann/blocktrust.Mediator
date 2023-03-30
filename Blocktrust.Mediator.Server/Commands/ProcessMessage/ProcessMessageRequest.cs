namespace Blocktrust.Mediator.Server.Commands.ProcessMessage;

using Blocktrust.DIDComm.Model.UnpackResultModels;
using MediatR;

public class ProcessMessageRequest : IRequest<ProcessMessageResponse>
{
    /// <summary>
    /// Optional: the old sender
    /// </summary>
    public string? SenderOldDid { get; }
    
    /// <summary>
    /// Optional: A sender we could identify
    /// </summary>
    public string? SenderDid { get; }
    
    /// <summary>
    /// The url of currently running mediator
    /// </summary>
    public string HostUrl { get; }
    
    /// <summary>
    /// The unpacked message
    /// </summary>
    public UnpackResult UnpackResult { get; }


    public ProcessMessageRequest(string? senderOldDid, string? senderDid, string hostUrl, UnpackResult unpackResult)
    {
        if (!string.IsNullOrEmpty(senderOldDid) && string.IsNullOrEmpty(senderDid))
        {
            throw new Exception("Invalid arguments. A sender did must be provided if  old sender did is provided.");
        }
        
        SenderOldDid = senderOldDid;
        SenderDid = senderDid;
        HostUrl = hostUrl;
        UnpackResult = unpackResult;
    }
}