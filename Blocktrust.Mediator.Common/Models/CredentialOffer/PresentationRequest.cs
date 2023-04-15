namespace Blocktrust.Mediator.Common.Models.CredentialOffer;

using System.Text.Json.Serialization;

public class PresentationRequest
{
    [JsonPropertyName("challenge")]
    public string? Challenge { get; set; }
    
    [JsonPropertyName("nonce")]
    public string? Nonce { get; set; }
    
    [JsonPropertyName("domain")]
    public string? Domain { get; set; }
    
    [JsonConstructor]
    public PresentationRequest()
    {
        
    }
}