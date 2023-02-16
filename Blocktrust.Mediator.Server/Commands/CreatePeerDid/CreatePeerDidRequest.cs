namespace Blocktrust.Mediator.Server.Commands.CreatePeerDid;

using Server.Models;
using FluentResults;
using MediatR;
using PeerDID.Types;

/// <summary>
/// Request
/// </summary>
public class CreatePeerDidRequest : IRequest<Result<PeerDid>>
{
    /// <summary>
    /// Request
    /// </summary>
    public CreatePeerDidRequest(List<string> serviceRoutingKeys, int numberOfAgreementKeys = 1, int numberOfAuthenticationKeys = 1, string? serviceEndpoint = null)
    {
        ServiceEndpoint = serviceEndpoint;
        ServiceRoutingKeys = serviceRoutingKeys;
        NumberOfAgreementKeys = numberOfAgreementKeys;
        NumberOfAuthenticationKeys = numberOfAuthenticationKeys;
    }

    /// <summary>
    /// Agreement keys (X25519) are used for encryption 
    /// </summary>
    public int NumberOfAgreementKeys { get; }

    /// <summary>
    /// AuthenticationKeys (ED25519) are used for signing
    /// </summary>
    public int NumberOfAuthenticationKeys { get; }

    public string? ServiceEndpoint { get; } 

    public List<string> ServiceRoutingKeys { get; } = new();
}