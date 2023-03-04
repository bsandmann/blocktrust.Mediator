namespace Blocktrust.Mediator.Client.Commands.MediatorCoordinator.UpdateKeys;

using FluentResults;
using MediatR;

public class UpdateMediatorKeysRequest : IRequest<Result>
{
    public Uri MediatorEndpoint { get; }
    public string MediatorDid { get; }
    public string LocalDid { get; }
    public List<string> KeysToAdd { get; }
    public List<string> KeysToRemove { get; }

    public UpdateMediatorKeysRequest(Uri mediatorEndpoint, string mediatorDid, string localDid, List<string> keysToAdd, List<string> keysToRemove)
    {
        MediatorEndpoint = mediatorEndpoint;
        MediatorDid = mediatorDid;
        LocalDid = localDid;
        KeysToAdd = keysToAdd;
        KeysToRemove = keysToRemove;
    }
}