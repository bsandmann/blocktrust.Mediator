namespace Blocktrust.Mediator.Server.Protocols.TrustPing.Models;

using System.Text.Json;
using System.Text.Json.Serialization;
using Utils;

public class TrustPingRequest
{
    [JsonPropertyName("type")] public string Type { get; set; }
    [JsonPropertyName("id")] public string Id { get; set; }
    [JsonPropertyName("from")] public string From { get; set; }
    [JsonPropertyName("body")] public TrustPingRequestBody TrustPingRequestBody { get; set; }

    // For serialization
    public TrustPingRequest()
    {
    }

    public TrustPingRequest(string from, bool responseRequested = true)
    {
        this.Type = TrustPingCommon.ProtocolIdentifierURI;
        this.Id = Guid.NewGuid().ToString();
        this.From = from;
        this.TrustPingRequestBody = new TrustPingRequestBody(responseRequested: responseRequested);
    }

    public string Serialize()
    {
        return JsonSerializer.Serialize(this, JsonUtils.CommonSerializationOptions);
    }
}