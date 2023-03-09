namespace Blocktrust.Mediator.Server.Commands.DatabaseCommands.Secrets.SaveSecrets;

using Blocktrust.Common.Models.Secrets;
using FluentResults;
using MediatR;

public class SaveSecretRequest : IRequest<Result>
{
    public Secret Secret { get; }
    public string Kid { get; }

    public SaveSecretRequest(string kid, Secret secret)
    {
        Kid = kid;
        Secret = secret;
    }
}