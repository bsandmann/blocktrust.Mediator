namespace Blocktrust.Mediator.Common.Models.CredentialOffer;

using System.Text.Json.Serialization;

public class PresentationDefinitionFormatJwt
{
    [JsonPropertyName("alg")]
    public List<string>? Alg { get; set; }
    
    [JsonPropertyName("proof_type")]
    public List<string>? ProofType { get; set; }

    [JsonConstructor]
    public PresentationDefinitionFormatJwt()
    {
        
    }
}