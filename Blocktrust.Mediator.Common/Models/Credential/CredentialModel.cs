namespace Blocktrust.Mediator.Common.Models.Credential;

using System.Text.Json.Serialization;

public class CredentialModel
{
    [JsonPropertyName("iss")] public string Issuer { get; set; }

    [JsonPropertyName("sub")] public string Subject { get; set; }

    [JsonPropertyName("nbf")] public long NotBefore { get; set; }

    [JsonPropertyName("vc")] public object VC { get; set; }  
}