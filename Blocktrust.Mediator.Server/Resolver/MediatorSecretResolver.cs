namespace Blocktrust.Mediator.Server.Resolver;

using Blocktrust.Common.Models.Secrets;
using Blocktrust.Common.Resolver;
using Commands.Secrets.GetSecrets;
using Commands.Secrets.SaveSecrets;
using MediatR;

public class MediatorSecretResolver : ISecretResolver
{
    private readonly IMediator _mediator;

    public MediatorSecretResolver(IMediator mediator)
    {
        _mediator = mediator;
    }

    public Secret? FindKey(string kid)
    {
        var secretResults = _mediator.Send(new GetSecretsRequest(new List<string>() { kid })).Result;
        if (secretResults.IsFailed)
        {
            return null;
        }

        return secretResults.Value.FirstOrDefault();
    }

    public HashSet<string> FindKeys(List<string> kids)
    {
        var secretResults = _mediator.Send(new GetSecretsRequest(kids)).Result;
        return secretResults.Value.Select(p => p.Kid).ToHashSet();
    }

    public void AddKey(string kid, Secret secret)
    {
        var r = _mediator.Send(new SaveSecretRequest(kid: kid, secret: secret)).Result;
    }
}