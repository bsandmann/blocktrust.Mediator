namespace Blocktrust.Mediator.Common.Models.CredentialOffer;

using System.Text.Json.Serialization;

//TODO a every rough implementation of https://identity.foundation/presentation-exchange/spec/v2.0.0/#term:input-descriptor-objects

public class PresentationDefinition
{
    [JsonPropertyName("id")]
    public string Id { get; set; }
    
    //Optional
    [JsonPropertyName("name")]
    public string? Name { get; set; }
    
    //Optional
    [JsonPropertyName("purpose")]
    public string? Purpose { get; set; }
    
    //Optional
    [JsonPropertyName("format")]
    public PresentationDefinitionFormat? Format { get; set; }

    [JsonConstructor]
    public PresentationDefinition()
    {
        
    }
}