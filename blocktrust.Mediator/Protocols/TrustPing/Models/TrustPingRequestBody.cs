namespace Blocktrust.Mediator.Protocols.TrustPing.Models;

using System.Text.Json.Serialization;

public class TrustPingRequestBody
{
    [JsonPropertyName("response_requested")] 
    public bool ResponseRequested { get; set; }

    public TrustPingRequestBody()
    {
        
    }

    public TrustPingRequestBody(bool responseRequested)
    {
        this.ResponseRequested = responseRequested;
    }
}