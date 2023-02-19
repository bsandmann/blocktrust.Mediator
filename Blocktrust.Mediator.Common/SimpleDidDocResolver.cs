namespace Blocktrust.Mediator.Common;

using System.Text.Json;
using Blocktrust.Common.Models.DidDoc;
using Blocktrust.Common.Resolver;
using Blocktrust.PeerDID.DIDDoc;
using Blocktrust.PeerDID.PeerDIDCreateResolve;
using Blocktrust.PeerDID.Types;

public class SimpleDidDocResolver : IDidDocResolver
{
    private readonly Dictionary<string, DidDoc> _docs;

    public SimpleDidDocResolver(Dictionary<string, DidDoc> docs)
    {
        this._docs = docs;
    }

    public SimpleDidDocResolver(List<DidDoc> docs) : this(docs.ToDictionary(x => x.Did, x => x))
    {
    }

    public DidDoc? Resolve(string did)
    {
        if (_docs.ContainsKey(did))
        {
            return _docs[did];
        }
        else if (PeerDidCreator.IsPeerDid(did))
        {
            //TODO could be any kind
            var didDocJson = PeerDidResolver.ResolvePeerDid(new PeerDid(did), VerificationMaterialFormatPeerDid.Jwk);
            var didDoc = DidDocPeerDid.FromJson(didDocJson);
            var combinedVerificationMethodsOfInvitation = didDoc.Authentications.Concat(didDoc.KeyAgreements);

            //TODO this should be done in here
            this.AddDoc(new DidDoc()
            {
                Did = didDoc.Did,
                KeyAgreements = didDoc.KeyAgreements.Select(p => p.Id).ToList(),
                Authentications = didDoc.Authentications.Select(p => p.Id).ToList(),
                VerificationMethods = combinedVerificationMethodsOfInvitation.Select(p => new VerificationMethod(
                    id: p.Id,
                    type: VerificationMethodType.JsonWebKey2020,
                    verificationMaterial: new VerificationMaterial(
                        format: VerificationMaterialFormat.Jwk,
                        value: JsonSerializer.Serialize((PeerDidJwk)p.VerMaterial.Value)),
                    controller: p.Controller
                )).ToList(),
                Services = new List<Service>()
            });
            return _docs[did]; 
        }
        else
        {
            return null;
        }
    }

    public void AddDoc(DidDoc doc)
    {
        _docs.Add(doc.Did, doc);
    }
}