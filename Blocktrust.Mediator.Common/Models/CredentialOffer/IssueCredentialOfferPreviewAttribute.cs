namespace Blocktrust.Mediator.Common.Models.CredentialOffer;

using System.Text.Json.Serialization;

public class IssueCredentialOfferPreviewAttribute
{
    [JsonPropertyName("name")] public string Name { get; set; }

    [JsonPropertyName("mime-type")] public string? MimeType { get; set; } // if mimetype is null, then the value is a string, otherwise it is a base64 encoded string

    [JsonPropertyName("value")] public string Value { get; set; }

    [JsonConstructor]
    public IssueCredentialOfferPreviewAttribute()
    {
        
    }
}