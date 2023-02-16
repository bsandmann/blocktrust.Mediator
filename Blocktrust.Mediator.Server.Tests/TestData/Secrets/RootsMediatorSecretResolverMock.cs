namespace Blocktrust.Mediator.Server.Tests.TestData.Secrets;

using DIDComm_v2.Common.Types;
using DIDComm_v2.Secrets;

public class RootsMediatorSecretResolverMock : SecretResolverInMemoryMock
{
    private static List<Secret> secrets = new List<Secret>()
    {
      
        new Secret(
            kid: "did:peer:2.Ez6LSk8oEwmAfG1JyV4oG9JrUuswJobRhx4RkVsc7uaAYirYK",
            type: VerificationMethodType.JSON_WEB_KEY_2020,
            verificationMaterial: new VerificationMaterial()
            {
                format = VerificationMaterialFormat.JWK,
                value = """
                        {
                           "kty":"OKP",
                           "d":"iDtzlOiq3bf2d5xr1eeruzzOkd5nPtoc7dJH-jhpeFg",
                           "crv":"X25519",
                           "x":"fa9jUmApsVOyeS0Erbb0b-silyC7bzpdRckZmSWBzzQ"
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
        return secrets.Select(secret => secret.kid).ToList();
    }

    public Secret? findKey(String kid)
    {
        return _secretResolverInMemory.findKey(kid);
    }

    public HashSet<String> findKeys(List<String> kids)
    {
        return _secretResolverInMemory.findKeys(kids);
    }
}