namespace Blocktrust.Mediator.Server.Commands.DatabaseCommands.GetRegisteredRecipients;

using Blocktrust.Mediator.Server.Models;
using FluentResults;
using MediatR;

public class GetRegisteredRecipientsRequest  : IRequest<Result<List<KeyEntryModel>>>
{
    public string RemoteDid { get; }

    public GetRegisteredRecipientsRequest(string remoteDid)
    {
        RemoteDid = remoteDid;
    }
}
