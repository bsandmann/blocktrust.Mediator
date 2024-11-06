using System.Text.Json;
using Blocktrust.Common.Models.DidDoc;
using Blocktrust.Common.Resolver;
using Blocktrust.DIDComm.Utils;
using Blocktrust.PeerDID.DIDDoc;
using Blocktrust.PeerDID.PeerDIDCreateResolve;
using Blocktrust.PeerDID.Types;

namespace Blocktrust.Mediator.Common;

public class SimpleDidDocResolver : IDidDocResolver
{
    private readonly Dictionary<string, DidDoc> _docs;

    public SimpleDidDocResolver(Dictionary<string, DidDoc> docs)
    {
        _docs = docs;
    }

    public SimpleDidDocResolver()
    {
        _docs = new Dictionary<string, DidDoc>();
    }

    public SimpleDidDocResolver(List<DidDoc> docs) : this(docs.ToDictionary(x => x.Did, x => x))
    {
    }

    public async Task<DidDoc?> Resolve(string did)
    {
        if (_docs.ContainsKey(did))
        {
            return _docs[did];
        }

        if (PeerDidCreator.IsPeerDid(did))
        {
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

            var combinedVerificationMethodsOfInvitation = didDocResult.Value.Authentications
                .Concat(didDocResult.Value.KeyAgreements);

            var services = new List<Service>();
            if (didDocResult.Value.Services != null)
            {
                foreach (var peerDidService in didDocResult.Value.Services)
                {
                    // Transform service from peer DID format to DIDComm format
                    var serviceEndpoint = TransformServiceEndpoint(peerDidService.ServiceEndpoint);
                    
                    var service = new Service(
                        id: peerDidService.Id,
                        serviceEndpoint: serviceEndpoint,
                        type: peerDidService.Type == "dm" ? 
                            ServiceConstants.ServiceDidcommMessaging : 
                            peerDidService.Type
                    );
                    services.Add(service);
                }
            }

            var didDoc = new DidDoc
            {
                Did = didDocResult.Value.Did,
                KeyAgreements = didDocResult.Value.KeyAgreements.Select(p => p.Id).ToList(),
                Authentications = didDocResult.Value.Authentications.Select(p => p.Id).ToList(),
                VerificationMethods = combinedVerificationMethodsOfInvitation.Select(p => new VerificationMethod(
                    id: p.Id,
                    type: VerificationMethodType.JsonWebKey2020,
                    verificationMaterial: new VerificationMaterial(
                        format: VerificationMaterialFormat.Jwk,
                        value: JsonSerializer.Serialize((PeerDidJwk)p.VerMaterial.Value, SerializationOptions.UnsafeRelaxedEscaping)),
                    controller: p.Controller
                )).ToList(),
                Services = services
            };

            _docs.Add(did, didDoc);
            return didDoc;
        }

        return null;
    }

    private ServiceEndpoint TransformServiceEndpoint(ServiceEndpoint peerDidEndpoint)
    {
        var routingKeys = peerDidEndpoint.RoutingKeys ?? new List<string>();
        var accept = peerDidEndpoint.Accept ?? new List<string>();

        // Handle both abbreviated ("s") and full ("uri") formats
        var uri = peerDidEndpoint.Uri;
        if (string.IsNullOrEmpty(uri) && peerDidEndpoint is IDictionary<string, object> dict)
        {
            if (dict.ContainsKey("s"))
            {
                uri = dict["s"].ToString()!;
            }
        }

        // Transform shortened property names back to full names if needed
        if (peerDidEndpoint is IDictionary<string, object> endpointDict)
        {
            if (endpointDict.ContainsKey("r") && endpointDict["r"] is IEnumerable<string> r)
            {
                routingKeys = r.ToList();
            }
            if (endpointDict.ContainsKey("a") && endpointDict["a"] is IEnumerable<string> a)
            {
                accept = a.ToList();
            }
        }

        return new ServiceEndpoint(
            uri: uri,
            routingKeys: routingKeys,
            accept: accept
        );
    }

    public void AddDoc(DidDoc doc)
    {
        _docs[doc.Did] = doc;
    }
}