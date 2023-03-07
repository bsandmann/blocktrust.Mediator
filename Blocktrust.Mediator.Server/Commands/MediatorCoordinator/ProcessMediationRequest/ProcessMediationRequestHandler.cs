namespace Blocktrust.Mediator.Server.Commands.MediatorCoordinator.ProcessMediationRequest;

using Blocktrust.DIDComm.Message.Messages;
using Blocktrust.Mediator.Common.Commands.CreatePeerDid;
using Blocktrust.Mediator.Common.Protocols;
using DatabaseCommands.GetConnection;
using DatabaseCommands.UpdateConnection;
using FluentResults;
using MediatR;

public class ProcessMediationRequestHandler : IRequestHandler<ProcessMediationRequestRequest, Result<Message>>
{
    private readonly IMediator _mediator;

    /// <summary>
    /// Constructor
    /// </summary>
    public ProcessMediationRequestHandler(IMediator mediator)
    {
        this._mediator = mediator;
    }

    /// <inheritdoc />
    public async Task<Result<Message>> Handle(ProcessMediationRequestRequest requestRequest, CancellationToken cancellationToken)
    {
        // If we already have a mediation, we deny the request
        var existingConnection = await _mediator.Send(new GetConnectionRequest(requestRequest.SenderDid, requestRequest.MediatorDid));
        if (existingConnection.IsFailed)
        {
            
        }

        if (existingConnection.Value is not null && existingConnection.Value.MediationGranted)
        {
            // we already have a mediation. deny the request
            var mediateDenyMessage = new MessageBuilder(
                    id: Guid.NewGuid().ToString(),
                    type: ProtocolConstants.CoordinateMediation2Deny,
                    body: new Dictionary<string, object>()
                )
                .fromPrior(requestRequest.FromPrior)
                .build();
            return Result.Ok(mediateDenyMessage);
        }
        else
        {
            var routingDidResult = await _mediator.Send(new CreatePeerDidRequest(serviceEndpoint: new Uri(requestRequest.HostUrl)), cancellationToken);
            if (routingDidResult.IsFailed)
            {
                //TODO handle error
            }
            
            var updateConnetionResult = await _mediator.Send(new UpdateConnectionMediationRequest(
                mediatorDid: requestRequest.MediatorDid,
                remoteDid: requestRequest.SenderDid,
                routingDid: routingDidResult.Value.PeerDid.Value,
                mediatorEndpoint: requestRequest.HostUrl,
                mediationGranted: true
            ), cancellationToken);

            if (updateConnetionResult.IsFailed)
            {
                //TODO handle error
            }

            // Create the grant mediation message
            var mediateGrantMessage = new MessageBuilder(
                    id: Guid.NewGuid().ToString(),
                    type: ProtocolConstants.CoordinateMediation2Grant,
                    body: new Dictionary<string, object>()
                    {
                        { "routing_did", routingDidResult.Value.PeerDid.Value }
                    }
                )
                .fromPrior(requestRequest.FromPrior)
                .build();
            return Result.Ok(mediateGrantMessage);
        }
    }
}