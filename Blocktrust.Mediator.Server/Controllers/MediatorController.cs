namespace Blocktrust.Mediator.Server.Controllers;

using Blocktrust.Common.Resolver;
using Commands.DatabaseCommands.CreateConnection;
using Commands.DatabaseCommands.GetConnection;
using Commands.ForwardMessage;
using Commands.MediatorCoordinator.ProcessMediationRequest;
using Commands.MediatorCoordinator.ProcessQueryMediatorKeys;
using Commands.MediatorCoordinator.ProcessUpdateMediatorKeys;
using Commands.OutOfBand.CreateOobInvitation;
using Commands.OutOfBand.GetOobInvitation;
using Commands.Pickup.ProcessStatusRequest;
using Common.Commands.CreatePeerDid;
using Common.Protocols;
using DIDComm;
using DIDComm.Message.FromPriors;
using DIDComm.Message.Messages;
using DIDComm.Model.PackEncryptedParamsModels;
using DIDComm.Model.UnpackParamsModels;
using FluentResults;
using MediatR;
using Microsoft.AspNetCore.Mvc;

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
            var peerDidResponse = await _mediator.Send(new CreatePeerDidRequest(numberOfAgreementKeys: 1, numberOfAuthenticationKeys: 1, serviceEndpoint: new Uri(hostUrl), serviceRoutingKeys: new List<string>()));
            if (peerDidResponse.IsFailed)
            {
                return Problem(statusCode: 500, detail: peerDidResponse.Errors.First().Message);
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
        FromPrior? fromPrior = null;
        string mediatorDid;
        var existingConnection = await _mediator.Send(new GetConnectionRequest(senderOldDid));
        if (existingConnection.IsFailed)
        {
            //Internal error
        }

        if (existingConnection.Value is null)
        {
            // Create new connection
            var mediatorDidResult = await _mediator.Send(new CreatePeerDidRequest(serviceEndpoint: new Uri(hostUrl)));
            if (mediatorDidResult.IsFailed)
            {
                //TODO
            }

            var iss = unpacked.Value.Metadata.EncryptedTo.First().Split('#')[0]; // The current Did of the mediator the msg was send to
            var sub = mediatorDidResult.Value.PeerDid.Value; // The new Did of the mediator that will be used for future communication
            fromPrior = FromPrior.Builder(iss, sub).Build();

            var createConnectionResult = await _mediator.Send(new CreateConnectionRequest(mediatorDidResult.Value.PeerDid.Value, senderDid));
            if (createConnectionResult.IsFailed)
            {
                //TODO
            }

            mediatorDid = mediatorDidResult.Value.PeerDid.Value;
        }
        else
        {
            mediatorDid = existingConnection.Value.MediatorDid;
        }

        Result<Message> result = Result.Fail(string.Empty);
        switch (unpacked.Value.Message.Type)
        {
            case ProtocolConstants.CoordinateMediation2Request:
                result = await _mediator.Send(new ProcessMediationRequestRequest(unpacked.Value.Message, senderDid, mediatorDid, hostUrl, fromPrior));
                break;
            case ProtocolConstants.CoordinateMediation2KeylistUpdate:
                result = await _mediator.Send(new ProcessUpdateMediatorKeysRequest(unpacked.Value.Message, senderDid, mediatorDid, hostUrl, fromPrior));
                break;
            case ProtocolConstants.CoordinateMediation2KeylistQuery:
                result = await _mediator.Send(new ProcessQueryMediatorKeysRequest(unpacked.Value.Message, senderDid, mediatorDid, hostUrl, fromPrior));
                break;
            case ProtocolConstants.MessagePickup3StatusRequest:
                result = await _mediator.Send(new ProcessPickupStatusRequestRequest(unpacked.Value.Message, senderDid, mediatorDid, hostUrl, fromPrior));
                break;
            case ProtocolConstants.ForwardMessage:
            {
                result = await _mediator.Send(new ProcessForwardMessageRequest(unpacked.Value.Message, senderDid, mediatorDid, hostUrl, fromPrior));
                if (result.IsFailed)
                {
                    return BadRequest(result.Errors.FirstOrDefault().Message);
                }

                return Accepted();
            }
            default:
                return BadRequest("Not implemented message type");
        }

        if (result.IsFailed)
        {
            return BadRequest("bla");
        }

        var packResult = didComm.PackEncrypted(
            new PackEncryptedParamsBuilder(result.Value, to: senderDid)
                .From(mediatorDid)
                .ProtectSenderId(false)
                .BuildPackEncryptedParams()
        );

        return Ok(packResult.PackedMessage);
    }
}