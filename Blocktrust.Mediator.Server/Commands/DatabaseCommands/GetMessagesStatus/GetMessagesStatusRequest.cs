namespace Blocktrust.Mediator.Server.Commands.DatabaseCommands.GetMessagesStatus;

using FluentResults;
using MediatR;
using Models;

public class GetMessagesStatusRequest : IRequest<Result<MessagesStatusModel>>
{
    /// <summary>
    /// Required: The Did all th messages are connected to
    /// </summary>
    public string RemoteDid { get; }

    /// <summary>
    /// Required: The Mediator instance that was used to connect to the RemoteDID
    /// While this is not stricly required it adds a additional level of security
    /// </summary>
    public string MediatorDid { get; set; }
    
    /// <summary>
    /// Optional: Get the status only of messages related to that key
    /// </summary>
    public string? RecipientDid { get;  }


    public GetMessagesStatusRequest(string remoteDid, string mediatorDid, string? recipientDid)
    {
        RemoteDid = remoteDid;
        MediatorDid = mediatorDid;
        RecipientDid = recipientDid;
    }
}