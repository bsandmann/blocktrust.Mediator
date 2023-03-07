namespace Blocktrust.Mediator.Server.Commands.DatabaseCommands.DeleteMessages;

using FluentResults;
using MediatR;

public class DeleteMessagesRequest : IRequest<Result>
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
    /// Required: The List of messages to delete
    /// </summary>
    public List<string> MessageIds { get; }


    public DeleteMessagesRequest(string remoteDid, string mediatorDid, List<string> messageIds)
    {
        RemoteDid = remoteDid;
        MediatorDid = mediatorDid;
        MessageIds = messageIds;
    }
}