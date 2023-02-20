namespace Blocktrust.Mediator.Server.Tests.TestData.Secrets;

using Blocktrust.Common.Models.DidDoc;
using Blocktrust.Common.Models.Secrets;
using DIDComm.Secrets;

public class RootsMediatorSecretResolverMock : SecretResolverInMemoryMock
{
    private static List<Secret> secrets = new List<Secret>()
    {
      
        new Secret(
            kid: "did:peer:2.Ez6LSk8oEwmAfG1JyV4oG9JrUuswJobRhx4RkVsc7uaAYirYK",
            type: VerificationMethodType.JsonWebKey2020,
            verificationMaterial: new VerificationMaterial()
            {
                format = VerificationMaterialFormat.Jwk,
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