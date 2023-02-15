namespace Blocktrust.Mediator.Commands.CreateOob;

using Blocktrust.Mediator.Models;
using FluentResults;
using MediatR;

/// <summary>
/// Request
/// </summary>
public class CreateInitialOobDidRequest : IRequest<Result<OobModel>>
{
    /// <summary>
    /// Request
    /// </summary>
    /// <param name="did"></param>
    public CreateInitialOobDidRequest(string did)
    {
        Did = did;
    }

    public string Did { get; set; }
   
}