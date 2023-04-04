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
    public CreatePeerDidRequest(string serviceEndpointDid, int numberOfAgreementKeys = 1, int numberOfAuthenticationKeys = 1)
    {
        NumberOfAgreementKeys = numberOfAgreementKeys;
        NumberOfAuthenticationKeys = numberOfAuthenticationKeys;
        ServiceEndpointDid = serviceEndpointDid;
        ServiceEndpoint = null;
    }
    
    /// <summary>
    /// Request
    /// </summary>
    public CreatePeerDidRequest(int numberOfAgreementKeys = 1, int numberOfAuthenticationKeys = 1)
    {
        NumberOfAgreementKeys = numberOfAgreementKeys;
        NumberOfAuthenticationKeys = numberOfAuthenticationKeys;
        ServiceEndpointDid = null;
        ServiceEndpoint = null;
    }

    /// <summary>
    /// Request
    /// </summary>
    public CreatePeerDidRequest(Uri? serviceEndpoint, int numberOfAgreementKeys = 1, int numberOfAuthenticationKeys = 1)
    {
        NumberOfAgreementKeys = numberOfAgreementKeys;
        NumberOfAuthenticationKeys = numberOfAuthenticationKeys;
        ServiceEndpointDid = null;
        ServiceEndpoint = serviceEndpoint;
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
    /// Instead of defining a ServiceEndpoing and optionally RoutingKeys, you can also use an existing DID directly to force another format
    /// </summary>
    public string? ServiceEndpointDid { get; }

    public Uri? ServiceEndpoint { get; }
}