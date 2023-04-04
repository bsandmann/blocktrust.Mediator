namespace Blocktrust.Mediator.Client.BlocktrustIntegrationTests;

using System.Text;
using System.Text.Json;
using Blocktrust.Common.Converter;
using Blocktrust.DIDComm.Secrets;
using Blocktrust.Mediator.Client.Commands.DiscoverFeatures;
using Blocktrust.Mediator.Client.Commands.MediatorCoordinator.QueryKeys;
using Blocktrust.Mediator.Common;
using Blocktrust.Mediator.Common.Commands.CreatePeerDid;
using Blocktrust.Mediator.Common.Models.OutOfBand;
using Blocktrust.PeerDID.DIDDoc;
using Blocktrust.PeerDID.PeerDIDCreateResolve;
using Blocktrust.PeerDID.Types;
using Commands.SendMessage;
using Commands.TrustPing;
using Common.Protocols;
using FluentAssertions;
using Xunit;

public class DebugTests
{
    private readonly HttpClient _httpClient;
    private SendMessageHandler _sendMessageHandler;
    private CreatePeerDidHandler _createPeerDidHandler;
    private readonly string _blocktrustMediatorUri = "https://localhost:7037/";

    public DebugTests()
    {
        _httpClient = new HttpClient();
    }

  
    [Fact]
    public async Task ReadOobInvitation()
    {
        var oob = "eyJ0eXBlIjoiaHR0cHM6Ly9kaWRjb21tLm9yZy9vdXQtb2YtYmFuZC8yLjAvaW52aXRhdGlvbiIsImlkIjoiNWFjZjUwNmUtMjEzZi00ZWY2LWExM2EtMjI2M2IxODUwMGY2IiwiZnJvbSI6ImRpZDpwZWVyOjIuRXo2TFNjclFiSENXd3JWQmtBV3pGNlJuRDlwSlRSZjhKdmJoRXhGQ3NhMnZmMXBkdS5WejZNa3ZkR0RiTFJ2WHZQWlFCRlBGYnhnTDk3aEt5TXVqVEFSSlV4NXRlM2hrUkZ4LlNleUpwWkNJNkltNWxkeTFwWkNJc0luUWlPaUprYlNJc0luTWlPaUprYVdRNmNHVmxjam95TGtWNk5reFRabGxFTVZOemVuUjBTRU4yY0Zwb1JsVllaWFo1T0RGRU9WZ3pUVzlZYzBSVmExQnFWbFI1YzBKU1p6Z3VWbm8yVFd0MGRrWjFWVnBHT0V4Q09HRkNNazAwUkVWdWVtVTFPSGhpVW5kbFRuUnZObVExVFROblprTlRVazEzY2k1VFpYbEtjRnBEU1RaSmJUVnNaSGt4Y0ZwRFNYTkpibEZwVDJsS2EySlRTWE5KYmsxcFQybEtiMlJJVW5kamVtOTJUREl4YkZwSGJHaGtSemw1VEcxS2MySXlUbkprU0VveFl6TlJkVnBIVmpKTWVVbHpTVzVKYVU5c2RHUk1RMHBvU1dwd1lrbHRVbkJhUjA1MllsY3dkbVJxU1dsWVdEQWlMQ0p5SWpwYlhTd2lZU0k2V3lKa2FXUmpiMjF0TDNZeUlsMTkiLCJib2R5Ijp7ImFjY2VwdCI6WyJkaWRjb21tL3YyIl19fQ";
        var decocedOOb = Encoding.UTF8.GetString(Base64Url.Decode(oob));
        var deserializedOob = JsonSerializer.Deserialize<OobModel>(decocedOOb);
        var resolvedFrom = PeerDidResolver.ResolvePeerDid(new PeerDid(deserializedOob.From), VerificationMaterialFormatPeerDid.Jwk);
        var resolvedFromAsDidDoc = DidDocPeerDid.FromJson(resolvedFrom.Value);

        var endpointDid = resolvedFromAsDidDoc.Value.Services.FirstOrDefault().ServiceEndpoint;
        var endpointDidResolved = PeerDidResolver.ResolvePeerDid(new PeerDid(endpointDid), VerificationMaterialFormatPeerDid.Jwk);
        var endpointDidResolvedAsDidDoc = DidDocPeerDid.FromJson(endpointDidResolved.Value);
        
    }
    
    [Fact]
    public async Task ReadPeerDid()
    {
        var peerDid = "did:peer:2.Vz6MknFnZUtNCJHkv5KzF2udqfVFpWKnHLRhYajKCnmiq3Y6f.Ez6LSqzcRd1F43qimB45idBH7j1tNvh1483HfqFFNYZDigqX4.SeyJpZCI6IiNzZXJ2aWNlLTEiLCJ0IjoiZG0iLCJzIjpudWxsfQ";
        var resolvedFrom = PeerDidResolver.ResolvePeerDid(new PeerDid(peerDid), VerificationMaterialFormatPeerDid.Jwk);
        var resolvedFromAsDidDoc = DidDocPeerDid.FromJson(resolvedFrom.Value);

        var endpointDid = resolvedFromAsDidDoc.Value.Services.FirstOrDefault().ServiceEndpoint;
        var endpointDidResolved = PeerDidResolver.ResolvePeerDid(new PeerDid(endpointDid), VerificationMaterialFormatPeerDid.Jwk);
        var endpointDidResolvedAsDidDoc = DidDocPeerDid.FromJson(endpointDidResolved.Value);
        
    }
}