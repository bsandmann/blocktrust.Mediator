namespace Blocktrust.Mediator.Server.Commands.DatabaseCommands.GetConnection;

using Blocktrust.Mediator.Server.Models;
using FluentResults;
using MediatR;

public class GetConnectionRequest : IRequest<Result<MediatorConnectionModel>>
{
    /// <summary>
    /// DID of the party connected to the mediator
    /// </summary>
    /// 
    public string? RemoteDid { get; }
    /// <summary>
    /// The mediator did
    /// </summary>
    /// 
    public string? MediatorDid { get; }

    public GetConnectionRequest(string? remoteDid, string? mediatorDid)
    {
        RemoteDid = remoteDid;
        MediatorDid = mediatorDid;
    }
}