namespace Blocktrust.Mediator.Server.Controllers;

using System.Text.Json;
using Blocktrust.Common.Resolver;
using Commands.DatabaseCommands.CreateOobInvitation;
using Commands.DatabaseCommands.GetOobInvitation;
using Commands.DatabaseCommands.GetShortenedUrl;
using Commands.ProcessMessage;
using Common.Commands.CreatePeerDid;
using DIDComm;
using DIDComm.Model.PackEncryptedParamsModels;
using DIDComm.Model.UnpackParamsModels;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Models;

[ApiController]
public class OobController : ControllerBase
{
    private readonly ILogger<OobController> _logger;
    private readonly IMediator _mediator;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public OobController(ILogger<OobController> logger, IMediator mediator, IHttpContextAccessor httpContextAccessor, ISecretResolver secretResolver, IDidDocResolver didDocResolver)
    {
        _logger = logger;
        _mediator = mediator;
        _httpContextAccessor = httpContextAccessor;
    }

    // Note: the pure text oob-invitation and the redirects are handled here. The graphical representations 
    // /index /oob_qrcode, /oob_small_qrcode are all handled in the razor pages


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
    /// Endpoint to the out of band invitation
    /// </summary>
    /// <returns></returns>
    [HttpGet("/qr")]
    [HttpGet("/qr/{path}")]
    public async Task<ActionResult<string>> QrCodeRedirection(string _oobid, string? path)
    {
        // while we do ingest a possible path, we don't process it any further, since only the guid is relevant for the resolution
        var isParseable = Guid.TryParse(_oobid,out var shortenedUrlEntityId);
        if (!isParseable)
        {
            return BadRequest("The provided shortened-url is incorrect");
        }

        var result = await _mediator.Send(new GetShortenedUrlRequest(shortenedUrlEntityId));
        if (result.IsFailed)
        {
            return BadRequest($"Error processing the shortened-url: {result.Errors.FirstOrDefault().Message}");
        }

        return RedirectPermanent(result.Value);
    }
   
    
    /// <summary>
    /// Simple proof of concept endpoints which returns the request-string as content-string
    /// </summary>
    /// <returns></returns>
    [HttpGet("/identityWallet")]
    public Task<ActionResult<string>> IdentityWalletOutOfBandInvitation()
    {
        var hostUrl = string.Concat(_httpContextAccessor!.HttpContext.Request.Scheme, "://", _httpContextAccessor.HttpContext.Request.Host, _httpContextAccessor.HttpContext.Request.Path,_httpContextAccessor.HttpContext.Request.QueryString);
        return Task.FromResult<ActionResult<string>>(Ok(hostUrl));
    }
}