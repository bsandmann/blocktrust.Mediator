namespace Blocktrust.Mediator.Common;

using DIDComm_v2.Common.Types;
using DIDComm_v2.DidDocs;
using PeerDID.DIDDoc;

public class ConvertDidDocPeerToDidDoc
{
    public static DidDoc Convert(DidDocPeerDid didDocPeerDid)
    {
        // var didDoc = new DidDoc();
        // didDoc.Did = didDocPeerDid.Did;
        // didDoc.Authentications = new List<string>();
        // didDoc.KeyAgreements = new List<string>();
        // didDoc.VerificationMethods = new List<VerificationMethod>();
        // foreach (var peerDidVerificationMethod in didDocPeerDid.Authentication)
        // {
        //     didDoc.VerificationMethods.Add(new VerificationMethod(
        //         id: peerDidVerificationMethod.Id,
        //         type: VerificationMethodType.JSON_WEB_KEY_2020,
        //         verificationMaterial: new VerificationMaterial(
        //             format: peerDidVerificationMethod.VerMaterial.Format,
        //             value: peerDidVerificationMethod.VerMaterial.Value)
        //         ));
        // }
        //
        return null;
    }
}