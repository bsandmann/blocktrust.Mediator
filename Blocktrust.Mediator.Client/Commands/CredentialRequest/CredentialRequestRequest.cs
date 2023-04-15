namespace Blocktrust.Mediator.Client.Commands.CredentialRequest;

using Common.Models.CredentialOffer;
using FluentResults;
using MediatR;
using PeerDID.Types;

public class CredentialRequestRequest : IRequest<Result<CredentialRequestResponse>>
{
    /// <summary>
    /// Local PeerDID 
    /// </summary>
    public PeerDid LocalPeerDid { get; }

    /// <summary>
    /// The messageId 
    /// </summary>
    public string MessageId { get; }

    /// <summary>
    /// PeerDID of the PRISM agent
    /// </summary>
    public PeerDid PrismPeerDid { get; set; }

    public string SignedJwtCredentialRequest { get; set; }

    public CredentialRequestRequest(string messageId, PeerDid localPeerDid, PeerDid prismPeerDid, string signedJwtCredentialRequest)
    {
        MessageId = messageId;
        LocalPeerDid = localPeerDid;
        PrismPeerDid = prismPeerDid;
        SignedJwtCredentialRequest = signedJwtCredentialRequest;
    }
}