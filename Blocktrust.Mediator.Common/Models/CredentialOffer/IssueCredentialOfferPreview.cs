namespace Blocktrust.Mediator.Common.Models.CredentialOffer;

using System.Text.Json.Serialization;

public class IssueCredentialOfferPreview
{
    [JsonPropertyName("type")]
    public string Type { get; set; }
    
    [JsonPropertyName("attributes")]
    public List<IssueCredentialOfferPreviewAttribute> Attributes { get; set; } = new List<IssueCredentialOfferPreviewAttribute>();

    [JsonConstructor]
    public IssueCredentialOfferPreview()
    {
        
    }
}