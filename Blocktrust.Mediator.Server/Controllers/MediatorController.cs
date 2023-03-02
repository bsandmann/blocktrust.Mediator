namespace Blocktrust.Mediator.Server.Controllers;

using System.Reflection.Metadata;
using Blocktrust.Common.Models.Secrets;
using Blocktrust.Common.Resolver;
using Commands.Connections.CreateConnection;
using Commands.Connections.GetConnection;
using Commands.MediatorCoordinator.AnswerMediation;
using Commands.OutOfBand.CreateOobInvitation;
using Commands.OutOfBand.GetOobInvitation;
using Commands.Secrets.SaveSecrets;
using Common;
using Common.Commands.CreatePeerDid;
using Common.Protocols;
using DIDComm;
using DIDComm.Message.FromPriors;
using DIDComm.Message.Messages;
using DIDComm.Model.PackEncryptedParamsModels;
using DIDComm.Model.UnpackParamsModels;
using DIDComm.Secrets;
using FluentResults;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Resolver;

[ApiController]
public class MediatorController : ControllerBase
{
    private readonly ILogger<MediatorController> _logger;
    private readonly IMediator _mediator;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ISecretResolver _secretResolver;
    private readonly IDidDocResolver _didDocResolver;

    public MediatorController(ILogger<MediatorController> logger, IMediator mediator, IHttpContextAccessor httpContextAccessor, ISecretResolver secretResolver, IDidDocResolver didDocResolver)
    {
        _logger = logger;
        _mediator = mediator;
        _httpContextAccessor = httpContextAccessor;
        _secretResolver = secretResolver;
        _didDocResolver = didDocResolver;
    }

    /// <summary>
    /// Ping pong sanity check
    /// </summary>
    /// <returns></returns>
    [HttpGet("/ping")]
    public async Task<ActionResult<string>> Ping(string arg = "")
    {
        await Task.Delay(10);
        return Ok($"Pong {arg}");
    }

    /// <summary>
    /// Endpoint to the out of band invitation
    /// </summary>
    /// <returns></returns>
    [HttpGet("/oob_url")]
    public async Task<ActionResult<string>> OutOfBandInvitation()
    {
        var hostUrl = string.Concat(_httpContextAccessor!.HttpContext.Request.Scheme, "://", _httpContextAccessor.HttpContext.Request.Host);
        var existingInvitationResult = await _mediator.Send(new GetOobInvitationRequest(hostUrl));
        var invitation = string.Empty;
        if (existingInvitationResult.IsFailed)
        {
            var peerDidResponse = await _mediator.Send(new CreatePeerDidRequest(numberOfAgreementKeys: 1, numberOfAuthenticationKeys: 1, serviceEndpoint: hostUrl, serviceRoutingKeys: new List<string>()));
            if (peerDidResponse.IsFailed)
            {
                return Problem(statusCode: 500, detail: peerDidResponse.Errors.First().Message);
            }

            //TODO not so nice, but works for now
            var zippedAuthenticationKeysAndSecrets = peerDidResponse.Value.PrivateAuthenticationKeys
                .Zip(peerDidResponse.Value.DidDoc.Authentications
                    .Select(p => p.Id), (secret, kid) => new { secret = secret, publicKid = kid });
            foreach (var zipped in zippedAuthenticationKeysAndSecrets)
            {
                var kid = zipped.publicKid;
                var saveSecretResult = await _mediator.Send(new SaveSecretRequest(kid, zipped.secret));
                if (saveSecretResult.IsFailed)
                {
                    return Problem(statusCode: 500, detail: saveSecretResult.Errors.First().Message);
                }
            }

            var zippedAgreementKeysAndSecrets = peerDidResponse.Value.PrivateAgreementKeys
                .Zip(peerDidResponse.Value.DidDoc.KeyAgreements
                    .Select(p => p.Id), (secret, kid) => new { secret = secret, publicKid = kid });
            foreach (var zipped in zippedAgreementKeysAndSecrets)
            {
                var kid = zipped.publicKid;
                var saveSecretResult = await _mediator.Send(new SaveSecretRequest(kid, zipped.secret));
                if (saveSecretResult.IsFailed)
                {
                    return Problem(statusCode: 500, detail: saveSecretResult.Errors.First().Message);
                }
            }

            var result = await _mediator.Send(new CreateOobInvitationRequest(hostUrl, peerDidResponse.Value.PeerDid));
            if (result.IsFailed)
            {
                return Problem(statusCode: 500, detail: result.Errors.First().Message);
            }

            invitation = result.Value.Invitation;
        }
        else
        {
            invitation = existingInvitationResult.Value.Invitation;
        }

        var invitationUrl = string.Concat(hostUrl, "?_oob=", invitation);
        return Ok(invitationUrl);
    }

    /// <summary>
    /// Mediator endpoint
    /// </summary>
    /// <returns></returns>
    [HttpPost("/")]
    public async Task<ActionResult<string>> Mediate()
    {
        var hostUrl = string.Concat(_httpContextAccessor!.HttpContext.Request.Scheme, "://", _httpContextAccessor.HttpContext.Request.Host);
        var request = _httpContextAccessor.HttpContext.Request;
        var body = await new StreamReader(request.Body).ReadToEndAsync();

        var f = _secretResolver.FindKey("asdf");

        var didComm = new DidComm(_didDocResolver, _secretResolver);
        var unpacked = didComm.Unpack(
            new UnpackParamsBuilder(body).BuildUnpackParams()
        );
        if (unpacked.IsFailed)
        {
            return BadRequest("Unable to unpack message");
        }

        string senderOldDid;
        string senderDid;
        if (unpacked.Value.Message.FromPrior is not null)
        {
            //TODO assertiongs that thease are not emtpy??
            senderOldDid = unpacked.Value.Message.FromPrior.Iss;
            senderDid = unpacked.Value.Message.FromPrior.Sub;
        }
        else
        {
            senderDid = unpacked.Value.Metadata.EncryptedFrom.Split("#")[0];
            senderOldDid = senderDid;
        }

        // Check for existing connection
        Result<CreatePeerDidResponse> mediatorDid = null;
        FromPrior? fromPrior = null;
        var existingConnection = await _mediator.Send(new GetConnectionRequest(senderOldDid));
        if (existingConnection.IsFailed)
        {
            //Internal error
        }

        if (existingConnection.Value is null)
        {
            // Create new connection
            mediatorDid = await _mediator.Send(new CreatePeerDidRequest(serviceEndpoint: hostUrl));
            if (mediatorDid.IsFailed)
            {
                //TODO
            }

            //TODO move the adding of keys to the secret resolver inside the CreatePeerDidHandler
            //This is a rather trashy implementation
            var zippedAgreementKeysAndSecrets = mediatorDid.Value.PrivateAgreementKeys
                .Zip(mediatorDid.Value.DidDoc.KeyAgreements
                    .Select(p => p.Id), (secret, kid) => new { secret = secret, kid = kid });
            foreach (var zip in zippedAgreementKeysAndSecrets)
            {
                zip.secret.Kid = zip.kid;
                _secretResolver.AddKey(zip.kid, zip.secret);
            }

            var zippedAuthenticationKeysAndSecrets = mediatorDid.Value.PrivateAuthenticationKeys
                .Zip(mediatorDid.Value.DidDoc.Authentications
                    .Select(p => p.Id), (secret, kid) => new { secret = secret, kid = kid });
            foreach (var zip in zippedAuthenticationKeysAndSecrets)
            {
                zip.secret.Kid = zip.kid;
                _secretResolver.AddKey(zip.kid, zip.secret);
            }


            var iss = unpacked.Value.Metadata.EncryptedTo.First().Split('#')[0]; // The current Did of the mediator the msg was send to
            var sub = mediatorDid.Value.PeerDid.Value; // The new Did of the mediator that will be used for future communication
            fromPrior = FromPrior.Builder(iss, sub).Build();

            var createConnectionResult = await _mediator.Send(new CreateConnectionRequest(mediatorDid.Value.PeerDid.Value, senderDid));
            if (createConnectionResult.IsFailed)
            {
                //TODO
            }
        }

        Result<Message> result = Result.Fail(string.Empty);
        if (unpacked.Value.Message.Type == ProtocolConstants.CoordinateMediation2Request)
        {
            result = await _mediator.Send(new AnswerMediationRequest(unpacked.Value.Message, senderDid, mediatorDid.Value.PeerDid.Value, hostUrl, fromPrior));
        }

        if (result.IsFailed)
        {
            return BadRequest("bla");
        }

        var packResult = didComm.PackEncrypted(
            new PackEncryptedParamsBuilder(result.Value, to: senderDid)
                .From(mediatorDid.Value.PeerDid.Value)
                .ProtectSenderId(false)
                .BuildPackEncryptedParams()
        );

        return Ok(packResult.PackedMessage);
    }
}