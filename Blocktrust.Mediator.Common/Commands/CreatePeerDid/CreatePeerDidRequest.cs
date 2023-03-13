namespace Blocktrust.Mediator.Common.Commands.CreatePeerDid;

using FluentResults;
using MediatR;

/// <summary>
/// Request
/// </summary>
public class CreatePeerDidRequest : IRequest<Result<CreatePeerDidResponse>>
{
    /// <summary>
    /// Request
    /// </summary>
    public CreatePeerDidRequest(List<string>? serviceRoutingKeys = default, int numberOfAgreementKeys = 1, int numberOfAuthenticationKeys = 1, Uri? serviceEndpoint = null, string? serviceDid = null)
    {
        ServiceEndpoint = serviceEndpoint;
        ServiceRoutingKeys = serviceRoutingKeys;
        NumberOfAgreementKeys = numberOfAgreementKeys;
        NumberOfAuthenticationKeys = numberOfAuthenticationKeys;
        ServiceDid = serviceDid;
    }

    /// <summary>
    /// Agreement keys (X25519) are used for encryption 
    /// </summary>
    public int NumberOfAgreementKeys { get; }

    /// <summary>
    /// AuthenticationKeys (ED25519) are used for signing
    /// </summary>
    public int NumberOfAuthenticationKeys { get; }

    /// <summary>
    /// THe url to connect to the service
    /// </summary>
    public Uri? ServiceEndpoint { get; } 

    public List<string>? ServiceRoutingKeys { get; } = new();
    
    /// <summary>
    /// The DID of the service (e.g the mediatorDID). This DID is used
    /// by another party to send messages to our DID (to be forwarded to us)
    /// </summary>
    public string? ServiceDid { get; }
}
