namespace Blocktrust.Mediator.Server.Resolver;

using Blocktrust.Common.Models.Secrets;
using Blocktrust.Common.Resolver;
using Commands.DatabaseCommands.Secrets.GetSecrets;
using Commands.DatabaseCommands.Secrets.SaveSecrets;
using MediatR;

public class MediatorSecretResolver : ISecretResolver
{
    private readonly IMediator _mediator;

    public MediatorSecretResolver(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task<Secret?> FindKey(string kid)
    {
        var secretResults = await _mediator.Send(new GetSecretsRequest(new List<string>() { kid }));
        if (secretResults.IsFailed)
        {
            return null;
        }

        return secretResults.Value.FirstOrDefault();
    }

    public async Task<HashSet<string>> FindKeys(List<string> kids)
    {
        var secretResults =await _mediator.Send(new GetSecretsRequest(kids));
        return secretResults.Value.Select(p => p.Kid).ToHashSet();
    }

    public Task AddKey(string kid, Secret secret)
    {
        return _mediator.Send(new SaveSecretRequest(kid: kid, secret: secret));
    }
}