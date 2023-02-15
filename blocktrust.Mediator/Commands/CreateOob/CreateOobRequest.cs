namespace Blocktrust.Node.Commands.CreateBlock;

using FluentResults;
using Mediator.Models;
using MediatR;

/// <summary>
/// Request
/// </summary>
public class CreateOobRequest : IRequest<Result<OobModel>>
{
    /// <summary>
    /// Request
    /// </summary>
    /// <param name="did"></param>
    public CreateOobRequest(string did)
    {
        Did = did;
    }

    public string Did { get; set; }
   
}