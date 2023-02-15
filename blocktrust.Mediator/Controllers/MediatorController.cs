namespace Blocktrust.Mediator.Controllers;

using Commands.CreateOob;
using MediatR;
using Microsoft.AspNetCore.Mvc;

[ApiController]
public class MediatorController : ControllerBase
{
    private readonly ILogger<MediatorController> _logger;
    private readonly IMediator _mediator;

    public MediatorController(ILogger<MediatorController> logger, IMediator mediator)
    {
        _logger = logger;
        _mediator = mediator;
    }

    /// <summary>
    /// Ping pong sanity check
    /// </summary>
    /// <returns></returns>
    [HttpGet("/ping")]
    public async Task<ActionResult<string>> Ping(string arg = "")
    {
        await Task.Delay(10);
        var r = await _mediator.Send(new CreateInitialOobDidRequest("mydid"));

        return Ok($"Pong {arg}");
    }

    /// <summary>
    /// Mediator endpoint
    /// </summary>
    /// <returns></returns>
    [HttpPost("/")]
    public async Task<ActionResult<string>> Mediate()
    {
        return null;
    }
}