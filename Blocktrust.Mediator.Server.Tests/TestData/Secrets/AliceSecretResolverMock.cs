namespace Blocktrust.Mediator.Server.Tests.TestData.Secrets;

using Blocktrust.Common.Models.DidDoc;
using Blocktrust.Common.Models.Secrets;
using DIDComm.Secrets;

public class AliceSecretResolverMock : SecretResolverInMemoryMock
{
    private static List<Secret> secrets = new List<Secret>()
    {
        new Secret(
            kid: "did:example:alice#key-1",
            type: VerificationMethodType.JsonWebKey2020,
            verificationMaterial: new VerificationMaterial()
            {
                format = VerificationMaterialFormat.Jwk,
                value = """
                        {
                           "kty":"OKP",
                           "d":"pFRUKkyzx4kHdJtFSnlPA9WzqkDT1HWV0xZ5OYZd2SY",
                           "crv":"Ed25519",
                           "x":"G-boxFB6vOZBu-wXkm-9Lh79I8nf9Z50cILaOgKKGww"
                        }
                """
            }
        ),
        new Secret(
            kid: "did:example:alice#key-2",
            type: VerificationMethodType.JsonWebKey2020,
            verificationMaterial: new VerificationMaterial()
            {
                format = VerificationMaterialFormat.Jwk,
                value =   """
                        {
                           "kty":"EC",
                           "d":"7TCIdt1rhThFtWcEiLnk_COEjh1ZfQhM4bW2wz-dp4A",
                           "crv":"P-256",
                           "x":"2syLh57B-dGpa0F8p1JrO6JU7UUSF6j7qL-vfk1eOoY",
                           "y":"BgsGtI7UPsObMRjdElxLOrgAO9JggNMjOcfzEPox18w"
                        }
                """
            }
        ),
        new Secret(
            kid: "did:example:alice#key-3",
            type: VerificationMethodType.JsonWebKey2020,
            verificationMaterial: new VerificationMaterial()
            {
                format = VerificationMaterialFormat.Jwk,
                value =  """
                        {
                           "kty":"EC",
                           "d":"N3Hm1LXA210YVGGsXw_GklMwcLu_bMgnzDese6YQIyA",
                           "crv":"secp256k1",
                           "x":"aToW5EaTq5mlAf8C5ECYDSkqsJycrW-e1SQ6_GJcAOk",
                           "y":"JAGX94caA21WKreXwYUaOCYTBMrqaX4KWIlsQZTHWCk"
                        }
                """
            }
        ),
        new Secret(
            kid: "did:example:alice#key-x25519-1",
            type: VerificationMethodType.JsonWebKey2020,
            verificationMaterial: new VerificationMaterial()
            {
                format = VerificationMaterialFormat.Jwk,
                value = """
                        {
                           "kty":"OKP",
                           "d":"r-jK2cO3taR8LQnJB1_ikLBTAnOtShJOsHXRUWT-aZA",
                           "crv":"X25519",
                           "x":"avH0O2Y4tqLAq8y9zpianr8ajii5m4F_mICrzNlatXs"
                        }
                """
            }
        ),
        new Secret(
            kid: "did:example:alice#key-p256-1",
            type: VerificationMethodType.JsonWebKey2020,
            verificationMaterial: new VerificationMaterial()
            {
                format = VerificationMaterialFormat.Jwk,
                value = """
                        {
                           "kty":"EC",
                           "d":"sB0bYtpaXyp-h17dDpMx91N3Du1AdN4z1FUq02GbmLw",
                           "crv":"P-256",
                           "x":"L0crjMN1g0Ih4sYAJ_nGoHUck2cloltUpUVQDhF2nHE",
                           "y":"SxYgE7CmEJYi7IDhgK5jI4ZiajO8jPRZDldVhqFpYoo"
                        }
                """
            }
        ),
        new Secret(
            kid: "did:example:alice#key-p521-1",
            type: VerificationMethodType.JsonWebKey2020,
            verificationMaterial: new VerificationMaterial()
            {
                format = VerificationMaterialFormat.Jwk,
                value = """
                        {
                           "kty":"EC",
                           "d":"AQCQKE7rZpxPnX9RgjXxeywrAMp1fJsyFe4cir1gWj-8t8xWaM_E2qBkTTzyjbRBu-JPXHe_auT850iYmE34SkWi",
                           "crv":"P-521",
                           "x":"AHBEVPRhAv-WHDEvxVM9S0px9WxxwHL641Pemgk9sDdxvli9VpKCBdra5gg_4kupBDhz__AlaBgKOC_15J2Byptz",
                           "y":"AciGcHJCD_yMikQvlmqpkBbVqqbg93mMVcgvXBYAQPP-u9AF7adybwZrNfHWCKAQwGF9ugd0Zhg7mLMEszIONFRk"
                        }
                """
            }
        ),
    };
        
    private SecretResolverInMemory _secretResolverInMemory = new SecretResolverInMemory(secrets);

    public List<Secret> GetSecrets()
    {
        return secrets;
    }

    public List<String> GetSecretKids()
    {
        return secrets.Select(secret => secret.Kid).ToList();
    }

    public Secret? FindKey(String kid)
    {
        return _secretResolverInMemory.FindKey(kid);
    }

    public HashSet<String> FindKeys(List<String> kids)
    {
        return _secretResolverInMemory.FindKeys(kids);
    }
}