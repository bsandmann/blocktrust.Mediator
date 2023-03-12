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

    public SimpleDidDocResolver()
    {
        this._docs = new Dictionary<string, DidDoc>();
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
            var didDocJsonResult = PeerDidResolver.ResolvePeerDid(new PeerDid(did), VerificationMaterialFormatPeerDid.Jwk);
            if (didDocJsonResult.IsFailed)
            {
                return null;
            }

            var didDocResult = DidDocPeerDid.FromJson(didDocJsonResult.Value);
            if (didDocResult.IsFailed)
            {
                return null;
            }

            var combinedVerificationMethodsOfInvitation = didDocResult.Value.Authentications.Concat(didDocResult.Value.KeyAgreements);

            //TODO this should not be done in here
            this.AddDoc(new DidDoc()
            {
                Did = didDocResult.Value.Did,
                KeyAgreements = didDocResult.Value.KeyAgreements.Select(p => p.Id).ToList(),
                Authentications = didDocResult.Value.Authentications.Select(p => p.Id).ToList(),
                VerificationMethods = combinedVerificationMethodsOfInvitation.Select(p => new VerificationMethod(
                    id: p.Id,
                    type: VerificationMethodType.JsonWebKey2020,
                    verificationMaterial: new VerificationMaterial(
                        format: VerificationMaterialFormat.Jwk,
                        value: JsonSerializer.Serialize((PeerDidJwk)p.VerMaterial.Value)),
                    controller: p.Controller
                )).ToList(),
                Services = didDocResult.Value?.Services?.ToList() ?? new List<Service>()
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