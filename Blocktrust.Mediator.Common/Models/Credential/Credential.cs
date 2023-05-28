namespace Blocktrust.Mediator.Common.Models.Credential;

using System.Text.Json;
using System.Text.Json.Serialization;
using Blocktrust.Common.Converter;
using DIDComm.Message.Attachments;

public class Credential
{
    [JsonPropertyName("issuer")] public string Issuer { get; set; }
    [JsonPropertyName("subject")] public string Subject { get; set; }
    [JsonPropertyName("notBefore")] public DateTime NotBeforeUtc { get; set; }
    [JsonPropertyName("innerCredential")] public InnerCredential InnerCredential { get; set; }
    [JsonPropertyName("jwt")] public string Jwt { get; set; }


    public Credential Parse(string jwt)
    {
        Jwt = jwt;
        var jwtParts = jwt.Split('.');
        var payloadJwt = jwtParts[1];
        var payload = Base64Url.Decode(payloadJwt);
        var credentialTempRaw = JsonSerializer.Deserialize<CredentialModel>(payload);

        Issuer = credentialTempRaw.Issuer;
        Subject = credentialTempRaw.Subject;
        NotBeforeUtc = DateTimeOffset.FromUnixTimeSeconds(credentialTempRaw.NotBefore).UtcDateTime;

        var innerJsonElement = (JsonElement)credentialTempRaw.VC;
        var innerCredential = new InnerCredential();

        var hasCredentialSubject = innerJsonElement.TryGetProperty("credentialSubject", out var credentialSubject);
        if (hasCredentialSubject)
        {
            var credentialSubjectJsonElement = (JsonElement)credentialSubject;
            var claims = new Dictionary<string, string>();
            foreach (var property in credentialSubjectJsonElement.EnumerateObject())
            {
                claims.Add(property.Name, property.Value.GetString());
            }

            if (claims.ContainsKey("id"))
            {
                innerCredential.Subject = claims["id"];
            }

            innerCredential.Claims = claims.Where(p => p.Key != "id").ToDictionary(p => p.Key, p => p.Value);
        }

        var hasType = innerJsonElement.TryGetProperty("type", out var type);
        if (hasType)
        {
            var typeJsonElement = (JsonElement)type;
            var types = new List<string>();
            foreach (var property in typeJsonElement.EnumerateArray())
            {
                types.Add(property.GetString());
            }

            innerCredential.Type = types;
        }

        var hasContext = innerJsonElement.TryGetProperty("@context", out var context);
        if (hasContext)
        {
            var contextJsonElement = (JsonElement)context;
            var contexts = new List<string>();
            foreach (var property in contextJsonElement.EnumerateArray())
            {
                contexts.Add(property.GetString());
            }

            innerCredential.Context = contexts;
        }

        InnerCredential = innerCredential;

        return this;
    }
}