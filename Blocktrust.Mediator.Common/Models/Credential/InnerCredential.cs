namespace Blocktrust.Mediator.Common.Models.Credential;

using System.Text.Json.Serialization;

public class InnerCredential
{
    [JsonPropertyName("subject")] public string Subject { get; set; }
    [JsonPropertyName("claims")] public Dictionary<string, string> Claims { get; set; }
    [JsonPropertyName("type")] public List<string> Type { get; set; }
    [JsonPropertyName("context")] public List<string> Context { get; set; }
}