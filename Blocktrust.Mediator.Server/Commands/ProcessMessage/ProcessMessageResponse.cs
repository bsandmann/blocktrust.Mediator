namespace Blocktrust.Mediator.Server.Commands.ProcessMessage;

using DIDComm.Message.Messages;

public class ProcessMessageResponse
{
    /// <summary>
    /// The response message
    /// </summary>
    public Message Message { get; }

    /// <summary>
    /// The Did of the current Mediator
    /// </summary>
    public string? MediatorDid { get; }

    /// <summary>
    /// The http request should be accepted (202)
    /// </summary>
    public bool RespondWithAccepted { get; }

    public ProcessMessageResponse(Message message, string? mediatorDid)
    {
        this.Message = message;
        this.MediatorDid = mediatorDid;
    }

    public ProcessMessageResponse()
    {
        RespondWithAccepted = true;
    }
}