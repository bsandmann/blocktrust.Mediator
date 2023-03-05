namespace Blocktrust.Mediator.Common.Commands.CreatePeerDid;

using FluentResults;
using MediatR;
using PeerDID.Types;

/// <summary>
/// Request
/// </summary>
public class CreatePeerDidRequest : IRequest<Result<CreatePeerDidResponse>>
{
    /// <summary>
    /// Request
    /// </summary>
    public CreatePeerDidRequest(List<string>? serviceRoutingKeys = default, int numberOfAgreementKeys = 1, int numberOfAuthenticationKeys = 1, Uri? serviceEndpoint = null)
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

    public Uri? ServiceEndpoint { get; } 

    public List<string>? ServiceRoutingKeys { get; } = new();
}