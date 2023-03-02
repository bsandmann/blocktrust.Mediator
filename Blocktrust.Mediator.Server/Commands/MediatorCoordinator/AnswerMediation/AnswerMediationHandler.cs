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

public class AnswerMediationHandler : IRequestHandler<AnswerMediationRequest, Result<Message>>
{
    private readonly DataContext _context;
    private readonly IMediator _mediator;
    private readonly ISecretResolver _secretResolver;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="context"></param>
    public AnswerMediationHandler(DataContext context, IMediator mediator, ISecretResolver secretResolver)
    {
        this._context = context;
        this._mediator = mediator;
        this._secretResolver = secretResolver;
    }

    public async Task<Result<Message>> Handle(AnswerMediationRequest request, CancellationToken cancellationToken)
    {
        //TODO handle the different cases: keylist update, keylist query

        // If we already have a mediation, we deny the request
        var existingConnection = await _mediator.Send(new GetConnectionRequest(request.SenderDid));
        if (existingConnection.IsFailed)
        {
            // database error
        }

        if (existingConnection.Value is not null && existingConnection.Value.MediationGranted)
        {
            // we already have a mediation
            // creat the deny message
        }
        else
        {
            var routingDidResult = await _mediator.Send(new CreatePeerDidRequest(serviceEndpoint: request.HostUrl), cancellationToken);
            if (routingDidResult.IsFailed)
            {
                //TODO handle error
            }
            
            //TODO move the adding of keys to the secret resolver inside the CreatePeerDidHandler
            //This is a rather trashy implementation
            var zippedAgreementKeysAndSecrets = routingDidResult.Value.PrivateAgreementKeys
                .Zip(routingDidResult.Value.DidDoc.KeyAgreements
                    .Select(p => p.Id), (secret, kid) => new { secret = secret, kid = kid });
            foreach (var zip in zippedAgreementKeysAndSecrets)
            {
                zip.secret.Kid = zip.kid;
                _secretResolver.AddKey(zip.kid, zip.secret);
            }
        
            var zippedAuthenticationKeysAndSecrets = routingDidResult.Value.PrivateAuthenticationKeys
                .Zip(routingDidResult.Value.DidDoc.Authentications
                    .Select(p => p.Id), (secret, kid) => new { secret = secret, kid = kid });
            foreach (var zip in zippedAuthenticationKeysAndSecrets)
            {
                zip.secret.Kid = zip.kid;
                _secretResolver.AddKey(zip.kid, zip.secret);
            }


            var updateConnetionResult = await _mediator.Send(new UpdateConnectionMediationRequest(
                mediatorDid: request.MediatorDid,
                remoteDid: request.SenderDid,
                routingDid: routingDidResult.Value.PeerDid.Value,
                mediatorEndpoint: request.HostUrl,
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
                .fromPrior(request.FromPrior)
                .build();
            return Result.Ok(mediateGrantMessage);
        }

        return Result.Ok();
    }
}