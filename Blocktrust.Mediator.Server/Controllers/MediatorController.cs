namespace Blocktrust.Mediator.Server.Controllers;

using Commands.CreateOobInvitation;
using Commands.CreatePeerDid;
using Commands.GetOobInvitation;
using Common.Commands.CreatePeerDid;
using MediatR;
using Microsoft.AspNetCore.Mvc;

[ApiController]
public class MediatorController : ControllerBase
{
    private readonly ILogger<MediatorController> _logger;
    private readonly IMediator _mediator;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public MediatorController(ILogger<MediatorController> logger, IMediator mediator, IHttpContextAccessor httpContextAccessor)
    {
        _logger = logger;
        _mediator = mediator;
        _httpContextAccessor = httpContextAccessor;
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
    /// Mediator endpoint
    /// </summary>
    /// <returns></returns>
    [HttpGet("/oob_url")]
    public async Task<ActionResult<string>> Mediate()
    {
        var hostUrl = string.Concat(_httpContextAccessor!.HttpContext.Request.Scheme, "://", _httpContextAccessor.HttpContext.Request.Host);
        var existingInvitationResult = await _mediator.Send(new GetOobInvitationRequest(hostUrl));
        var invitation = string.Empty;
        if (existingInvitationResult.IsFailed)
        {
            var peerDid = await _mediator.Send(new CreatePeerDidRequest(numberOfAgreementKeys: 1, numberOfAuthenticationKeys: 1, serviceEndpoint: hostUrl, serviceRoutingKeys: new List<string>()));
            var result = await _mediator.Send(new CreateOobInvitationRequest(hostUrl, peerDid.Value));
            invitation = result.Value.Invitation;
        }
        else
        {
            invitation = existingInvitationResult.Value.Invitation;
        }
        
        var invitationUrl = string.Concat(hostUrl,"?_oob=", invitation);
        return Ok(invitationUrl);
    }
}