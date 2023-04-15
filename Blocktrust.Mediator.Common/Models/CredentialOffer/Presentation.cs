namespace Blocktrust.Mediator.Common.Models.CredentialOffer;

using System.Text.Json.Serialization;

public class Presentation
{
    [JsonPropertyName("options")] public PresentationRequest PresentationRequest { get; set; }

    [JsonPropertyName("presentation_definition")]
    public PresentationDefinition PresentationDefinition { get; set; }

    [JsonConstructor]
    public Presentation()
    {
        
    }
}