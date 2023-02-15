namespace Blocktrust.Mediator.Controllers;

using Microsoft.AspNetCore.Mvc;

[ApiController]
public class MediatorController : ControllerBase
{
    private readonly ILogger<MediatorController> _logger;
    
    public MediatorController(ILogger<MediatorController> logger)
    {
        _logger = logger;
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
    [HttpPost("/")]
    public async Task<ActionResult<string>> Mediate()
    {
        return null;
    }
}