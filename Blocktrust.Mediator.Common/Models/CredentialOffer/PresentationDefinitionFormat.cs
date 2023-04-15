namespace Blocktrust.Mediator.Common.Models.CredentialOffer;

using System.Text.Json.Serialization;

public class PresentationDefinitionFormat
{
    [JsonPropertyName("jwt")]
    public PresentationDefinitionFormatJwt? Jwt { get; set; }

    [JsonConstructor]
    public PresentationDefinitionFormat()
    {
        
    }
    
}