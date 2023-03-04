namespace Blocktrust.Mediator.Server.Commands.MediatorCoordinator.AnswerMediation;

using System.Text.Json.Nodes;
using Blocktrust.Common.Resolver;
using Common.Commands.CreatePeerDid;
using Common.Protocols;
using Connections.CreateConnection;
using Connections.GetConnection;
using Connections.UpdateConnection;
using DIDComm.Message.Messages;
using Entities;
using FluentResults;
using MediatR;
using Microsoft.EntityFrameworkCore;

public class ProcessMediationRequestHandler : IRequestHandler<ProcessMediationRequestRequest, Result<Message>>
{
    private readonly DataContext _context;
    private readonly IMediator _mediator;
    private readonly ISecretResolver _secretResolver;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="context"></param>
    public ProcessMediationRequestHandler(DataContext context, IMediator mediator, ISecretResolver secretResolver)
    {
        this._context = context;
        this._mediator = mediator;
        this._secretResolver = secretResolver;
    }

    public async Task<Result<Message>> Handle(ProcessMediationRequestRequest requestRequest, CancellationToken cancellationToken)
    {
        //TODO handle the different cases: keylist update, keylist query

        // If we already have a mediation, we deny the request
        var existingConnection = await _mediator.Send(new GetConnectionRequest(requestRequest.SenderDid));
        if (existingConnection.IsFailed)
        {
            // database error
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
            var routingDidResult = await _mediator.Send(new CreatePeerDidRequest(serviceEndpoint: requestRequest.HostUrl), cancellationToken);
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

        return Result.Ok();
    }
}