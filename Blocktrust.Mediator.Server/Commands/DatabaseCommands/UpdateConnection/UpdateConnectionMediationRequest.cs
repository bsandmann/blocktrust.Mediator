namespace Blocktrust.Mediator.Server.Commands.Connections.UpdateConnection;

using FluentResults;
using MediatR;
using Models;

public class UpdateConnectionMediationRequest : IRequest<Result<MediatorConnectionModel>>
{
    public string MediatorDid { get; }
    public string RemoteDid { get; }
    public string RoutingDid { get; }
    public string MediatorEndpoint { get; }
    public bool MediationGranted { get; }

    public UpdateConnectionMediationRequest(string mediatorDid, string remoteDid, string routingDid, string mediatorEndpoint, bool mediationGranted)
    {
        MediatorDid = mediatorDid;
        RemoteDid = remoteDid;
        RoutingDid = routingDid;
        MediatorEndpoint = mediatorEndpoint;
        MediationGranted = mediationGranted;
    }
}