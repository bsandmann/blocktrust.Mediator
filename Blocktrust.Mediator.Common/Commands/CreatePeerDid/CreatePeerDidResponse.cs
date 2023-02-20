namespace Blocktrust.Mediator.Common.Commands.CreatePeerDid;

using Blocktrust.Common.Models.Secrets;
using PeerDID.DIDDoc;
using PeerDID.Types;

public class CreatePeerDidResponse
{
    public PeerDid PeerDid { get; }
    public DidDocPeerDid DidDoc { get; }
    public List<Secret> PrivateAgreementKeys { get; }
    public List<Secret> PrivateAuthenticationKeys { get; }

    public CreatePeerDidResponse(PeerDid peerDid, DidDocPeerDid didDoc, List<Secret> privateAgreementKeys, List<Secret> privateAuthenticationKeys)
    {
        PeerDid = peerDid;
        DidDoc = didDoc;
        PrivateAgreementKeys = privateAgreementKeys;
        PrivateAuthenticationKeys = privateAuthenticationKeys;
    }
}