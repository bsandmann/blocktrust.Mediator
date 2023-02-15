﻿namespace Blocktrust.Mediator.Tests.TestData.Secrets;

using DIDComm_v2.Common.Types;
using DIDComm_v2.Secrets;

public class BobSecretResolverMock : SecretResolverInMemoryMock
{
    private static List<Secret> secrets = new List<Secret>()
    {
        new Secret(
            kid: "did:example:bob#key-x25519-1",
            type: VerificationMethodType.JSON_WEB_KEY_2020,
            verificationMaterial: new VerificationMaterial()
            {
                format = VerificationMaterialFormat.JWK,
                value =  """
                        {
                           "kty":"OKP",
                           "d":"b9NnuOCB0hm7YGNvaE9DMhwH_wjZA1-gWD6dA0JWdL0",
                           "crv":"X25519",
                           "x":"GDTrI66K0pFfO54tlCSvfjjNapIs44dzpneBgyx0S3E"
                        }
                """
            }
        ),
        new Secret(
            kid:"did:example:bob#key-x25519-2",
            type: VerificationMethodType.JSON_WEB_KEY_2020,
            verificationMaterial: new VerificationMaterial()
            {
                format = VerificationMaterialFormat.JWK,
                value =    """
                        {
                           "kty":"OKP",
                           "d":"p-vteoF1gopny1HXywt76xz_uC83UUmrgszsI-ThBKk",
                           "crv":"X25519",
                           "x":"UT9S3F5ep16KSNBBShU2wh3qSfqYjlasZimn0mB8_VM"
                        }
                """
            }
        ),
        new Secret(
            kid: "did:example:bob#key-x25519-3",
            type: VerificationMethodType.JSON_WEB_KEY_2020,
            verificationMaterial: new VerificationMaterial()
            {
                format = VerificationMaterialFormat.JWK,
                value =  """
                        {
                           "kty":"OKP",
                           "d":"f9WJeuQXEItkGM8shN4dqFr5fLQLBasHnWZ-8dPaSo0",
                           "crv":"X25519",
                           "x":"82k2BTUiywKv49fKLZa-WwDi8RBf0tB0M8bvSAUQ3yY"
                        }
                """
            }
        ),
        new Secret(
            kid: "did:example:bob#key-p256-1",
            type: VerificationMethodType.JSON_WEB_KEY_2020,
            verificationMaterial: new VerificationMaterial()
            {
                format = VerificationMaterialFormat.JWK,
                value = """
                        {
                           "kty":"EC",
                           "d":"PgwHnlXxt8pwR6OCTUwwWx-P51BiLkFZyqHzquKddXQ",
                           "crv":"P-256",
                           "x":"FQVaTOksf-XsCUrt4J1L2UGvtWaDwpboVlqbKBY2AIo",
                           "y":"6XFB9PYo7dyC5ViJSO9uXNYkxTJWn0d_mqJ__ZYhcNY"
                        }
                """
            }
        ),
        new Secret(
            kid: "did:example:bob#key-p256-2",
            type: VerificationMethodType.JSON_WEB_KEY_2020,
            verificationMaterial: new VerificationMaterial()
            {
                format = VerificationMaterialFormat.JWK,
                value = """
                        {
                           "kty":"EC",
                           "d":"agKz7HS8mIwqO40Q2dwm_Zi70IdYFtonN5sZecQoxYU",
                           "crv":"P-256",
                           "x":"n0yBsGrwGZup9ywKhzD4KoORGicilzIUyfcXb1CSwe0",
                           "y":"ov0buZJ8GHzV128jmCw1CaFbajZoFFmiJDbMrceCXIw"
                        }
                """
            }
        ),
        new Secret(
            kid: "did:example:bob#key-p384-1",
            type: VerificationMethodType.JSON_WEB_KEY_2020,
            verificationMaterial: new VerificationMaterial()
            {
                format = VerificationMaterialFormat.JWK,
                value = """
                        {
                           "kty":"EC",
                           "d":"ajqcWbYA0UDBKfAhkSkeiVjMMt8l-5rcknvEv9t_Os6M8s-HisdywvNCX4CGd_xY",
                           "crv":"P-384",
                           "x":"MvnE_OwKoTcJVfHyTX-DLSRhhNwlu5LNoQ5UWD9Jmgtdxp_kpjsMuTTBnxg5RF_Y",
                           "y":"X_3HJBcKFQEG35PZbEOBn8u9_z8V1F9V1Kv-Vh0aSzmH-y9aOuDJUE3D4Hvmi5l7"
                        }
                """
            }
        ),
        new Secret(
            kid:"did:example:bob#key-p384-2",
            type: VerificationMethodType.JSON_WEB_KEY_2020,
            verificationMaterial: new VerificationMaterial()
            {
                format = VerificationMaterialFormat.JWK,
                value =  """
                        {
                           "kty":"EC",
                           "d":"OiwhRotK188BtbQy0XBO8PljSKYI6CCD-nE_ZUzK7o81tk3imDOuQ-jrSWaIkI-T",
                           "crv":"P-384",
                           "x":"2x3HOTvR8e-Tu6U4UqMd1wUWsNXMD0RgIunZTMcZsS-zWOwDgsrhYVHmv3k_DjV3",
                           "y":"W9LLaBjlWYcXUxOf6ECSfcXKaC3-K9z4hCoP0PS87Q_4ExMgIwxVCXUEB6nf0GDd"
                        }
                """
            }
        ),
        new Secret(
            kid: "did:example:bob#key-p521-1",
            type: VerificationMethodType.JSON_WEB_KEY_2020,
            verificationMaterial: new VerificationMaterial()
            {
                format = VerificationMaterialFormat.JWK,
                value = """
                        {
                           "kty":"EC",
                           "d":"AV5ocjvy7PkPgNrSuvCxtG70NMj6iTabvvjSLbsdd8OdI9HlXYlFR7RdBbgLUTruvaIRhjEAE9gNTH6rWUIdfuj6",
                           "crv":"P-521",
                           "x":"Af9O5THFENlqQbh2Ehipt1Yf4gAd9RCa3QzPktfcgUIFADMc4kAaYVViTaDOuvVS2vMS1KZe0D5kXedSXPQ3QbHi",
                           "y":"ATZVigRQ7UdGsQ9j-omyff6JIeeUv3CBWYsZ0l6x3C_SYqhqVV7dEG-TafCCNiIxs8qeUiXQ8cHWVclqkH4Lo1qH"
                        }
                """
            }
        ),
        new Secret(
            kid: "did:example:bob#key-p521-2",
            type: VerificationMethodType.JSON_WEB_KEY_2020,
            verificationMaterial: new VerificationMaterial()
            {
                format = VerificationMaterialFormat.JWK,
                value =   """
                        {
                           "kty":"EC",
                           "d":"ABixMEZHsyT7SRw-lY5HxdNOofTZLlwBHwPEJ3spEMC2sWN1RZQylZuvoyOBGJnPxg4-H_iVhNWf_OtgYODrYhCk",
                           "crv":"P-521",
                           "x":"ATp_WxCfIK_SriBoStmA0QrJc2pUR1djpen0VdpmogtnKxJbitiPq-HJXYXDKriXfVnkrl2i952MsIOMfD2j0Ots",
                           "y":"AEJipR0Dc-aBZYDqN51SKHYSWs9hM58SmRY1MxgXANgZrPaq1EeGMGOjkbLMEJtBThdjXhkS5VlXMkF0cYhZELiH"
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