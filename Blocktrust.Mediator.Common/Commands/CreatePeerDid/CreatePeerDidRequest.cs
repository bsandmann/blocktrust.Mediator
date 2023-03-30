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
    public CreatePeerDidRequest(List<string>? serviceRoutingKeys = default, int numberOfAgreementKeys = 1, int numberOfAuthenticationKeys = 1, Uri? serviceEndpoint = null)
    {
        ServiceEndpoint = serviceEndpoint;
        ServiceRoutingKeys = serviceRoutingKeys;
        NumberOfAgreementKeys = numberOfAgreementKeys;
        NumberOfAuthenticationKeys = numberOfAuthenticationKeys;
        ServiceEndpointDid = null;
    }
    
    /// <summary>
    /// Request
    /// </summary>
    public CreatePeerDidRequest(string serviceEndpointDid, int numberOfAgreementKeys = 1, int numberOfAuthenticationKeys = 1)
    {
        ServiceEndpoint = null;
        ServiceRoutingKeys = null;
        NumberOfAgreementKeys = numberOfAgreementKeys;
        NumberOfAuthenticationKeys = numberOfAuthenticationKeys;
        ServiceEndpointDid = serviceEndpointDid;
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

    /// <summary>
    /// RoutingKeys of the mediator
    /// </summary>
    public List<string>? ServiceRoutingKeys { get; } = new();
    
    /// <summary>
    /// Instead of defining a ServiceEndpoing and optionally RoutingKeys, you can also use an existing DID directly to force another format
    /// </summary>
    public string? ServiceEndpointDid { get; }
}
