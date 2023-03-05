namespace Blocktrust.Mediator.Server.Commands.Secrets.GetSecrets;

using Blocktrust.Common.Models.Secrets;
using FluentResults;
using MediatR;

public class GetSecretsRequest : IRequest<Result<List<Secret>>>
{
    public List<string> Kids { get; set; }

    public GetSecretsRequest(List<string> kids)
    {
        Kids = kids;
    }
}