namespace Blocktrust.Mediator.Server.Commands.DatabaseCommands.Secrets.GetSecrets;

using Blocktrust.Common.Models.Secrets;
using FluentResults;
using MediatR;

public class GetSecretsRequest : IRequest<Result<List<Secret>>>
{
    public List<string> Kids { get; }

    public GetSecretsRequest(List<string> kids)
    {
        Kids = kids;
    }
}