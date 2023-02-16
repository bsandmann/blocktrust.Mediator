namespace Blocktrust.Mediator.Server.Tests.TestData.DIDDoc;

using DIDComm_v2.Common.Types;
using DIDComm_v2.DidDocs;
using DIDComm_v2.ProtocolsRouting.Routing;

public class DIDDocRootsMediator
{
    public static readonly VerificationMethod RootsMediator_1 =
        new VerificationMethod(
            id: "did:peer:2.Ez6LSk8oEwmAfG1JyV4oG9JrUuswJobRhx4RkVsc7uaAYirYK.Vz6Mkgm5gQ13JisT9HPh7oQUsTeAHMWZoQzzsYD5oP2Y9rqCs#6LSk8oEwmAfG1JyV4oG9JrUuswJobRhx4RkVsc7uaAYirYK",
            controller: "did:peer:2.Ez6LSk8oEwmAfG1JyV4oG9JrUuswJobRhx4RkVsc7uaAYirYK.Vz6Mkgm5gQ13JisT9HPh7oQUsTeAHMWZoQzzsYD5oP2Y9rqCs#6LSk8oEwmAfG1JyV4oG9JrUuswJobRhx4RkVsc7uaAYirYK",
            type: VerificationMethodType.JSON_WEB_KEY_2020,
            verificationMaterial: new VerificationMaterial
            {
                format = VerificationMaterialFormat.JWK,
                value = """
            {
                "kty": "OKP",
                "crv": "X25519",
                "x": "fa9jUmApsVOyeS0Erbb0b-silyC7bzpdRckZmSWBzzQ"
            }
        """.Trim()
            }
        );
 
    public static DIDDoc DID_DOC_ROOTS_SPEC_TEST_VECTORS = new DIDDoc
    {
        did = "did:peer:2.Ez6LSk8oEwmAfG1JyV4oG9JrUuswJobRhx4RkVsc7uaAYirYK.Vz6Mkgm5gQ13JisT9HPh7oQUsTeAHMWZoQzzsYD5oP2Y9rqCs#6LSk8oEwmAfG1JyV4oG9JrUuswJobRhx4RkVsc7uaAYirYK",
        authentications = new List<string>
        {
            "did:peer:2.Ez6LSk8oEwmAfG1JyV4oG9JrUuswJobRhx4RkVsc7uaAYirYK.Vz6Mkgm5gQ13JisT9HPh7oQUsTeAHMWZoQzzsYD5oP2Y9rqCs#6LSk8oEwmAfG1JyV4oG9JrUuswJobRhx4RkVsc7uaAYirYK",
           
        },
        keyAgreements = new List<string>
        {
            "did:peer:2.Ez6LSk8oEwmAfG1JyV4oG9JrUuswJobRhx4RkVsc7uaAYirYK.Vz6Mkgm5gQ13JisT9HPh7oQUsTeAHMWZoQzzsYD5oP2Y9rqCs#6LSk8oEwmAfG1JyV4oG9JrUuswJobRhx4RkVsc7uaAYirYK",
        },
        didCommServices = new List<DIDCommService>
        {
            // new DIDCommService
            // (
            //     id : "did:example:123456789abcdefghi#didcomm-1",
            //     serviceEndpoint : "did:example:mediator1",
            //     accept : new List<string>
            //     {
            //         Routing.PROFILE_DIDCOMM_V2,
            //         Routing.PROFILE_DIDCOMM_AIP2_ENV_RFC587
            //     },
            //     routingKeys : new List<string>
            //     {
            //         "did:example:mediator2#key-p521-1"
            //     }
            // ) 
        },
        verificationMethods = new List<VerificationMethod>
        {
            RootsMediator_1,
        },
    };
}