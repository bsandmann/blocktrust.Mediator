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

public class BasicMessageAbTests
{
    private readonly HttpClient _httpClient;
    private SendMessageHandler _sendMessageHandler;
    private CreatePeerDidHandler _createPeerDidHandler;
    private readonly string _blocktrustMediatorUri = "https://localhost:7037/";

    public BasicMessageAbTests()
    {
        _httpClient = new HttpClient();
    }

    /// <summary>
    /// This tests assumes that the Roots Mediator is running on http://127.0.0.1:8000
    /// </summary>
    [Fact]
    public async Task BasicMessageAbTestSucceeds()
    {
        var ff = "eyJ0eXBlIjoiaHR0cHM6Ly9kaWRjb21tLm9yZy9vdXQtb2YtYmFuZC8yLjAvaW52aXRhdGlvbiIsImlkIjoiNWFjZjUwNmUtMjEzZi00ZWY2LWExM2EtMjI2M2IxODUwMGY2IiwiZnJvbSI6ImRpZDpwZWVyOjIuRXo2TFNjclFiSENXd3JWQmtBV3pGNlJuRDlwSlRSZjhKdmJoRXhGQ3NhMnZmMXBkdS5WejZNa3ZkR0RiTFJ2WHZQWlFCRlBGYnhnTDk3aEt5TXVqVEFSSlV4NXRlM2hrUkZ4LlNleUpwWkNJNkltNWxkeTFwWkNJc0luUWlPaUprYlNJc0luTWlPaUprYVdRNmNHVmxjam95TGtWNk5reFRabGxFTVZOemVuUjBTRU4yY0Zwb1JsVllaWFo1T0RGRU9WZ3pUVzlZYzBSVmExQnFWbFI1YzBKU1p6Z3VWbm8yVFd0MGRrWjFWVnBHT0V4Q09HRkNNazAwUkVWdWVtVTFPSGhpVW5kbFRuUnZObVExVFROblprTlRVazEzY2k1VFpYbEtjRnBEU1RaSmJUVnNaSGt4Y0ZwRFNYTkpibEZwVDJsS2EySlRTWE5KYmsxcFQybEtiMlJJVW5kamVtOTJUREl4YkZwSGJHaGtSemw1VEcxS2MySXlUbkprU0VveFl6TlJkVnBIVmpKTWVVbHpTVzVKYVU5c2RHUk1RMHBvU1dwd1lrbHRVbkJhUjA1MllsY3dkbVJxU1dsWVdEQWlMQ0p5SWpwYlhTd2lZU0k2V3lKa2FXUmpiMjF0TDNZeUlsMTkiLCJib2R5Ijp7ImFjY2VwdCI6WyJkaWRjb21tL3YyIl19fQ";
        var ecodedInvitation = Encoding.UTF8.GetString(Base64Url.Decode(ff));
        var ffoobModel = JsonSerializer.Deserialize<OobModel>(ecodedInvitation);
        var gsinvitationPeerDidResult = PeerDidResolver.ResolvePeerDid(new PeerDid(ffoobModel.From), VerificationMaterialFormatPeerDid.Jwk);
        var gsdinvitationPeerDidDocResult = DidDocPeerDid.FromJson(gsinvitationPeerDidResult.Value);

        var endpoint = gsdinvitationPeerDidDocResult.Value.Services.FirstOrDefault().ServiceEndpoint;
        var endpointr = PeerDidResolver.ResolvePeerDid(new PeerDid(endpoint.Uri), VerificationMaterialFormatPeerDid.Jwk);
        var f = DidDocPeerDid.FromJson(endpointr.Value);
        
        
        // Arrange
        // First get the OOB from the running mediator
        var response = await _httpClient.GetAsync(_blocktrustMediatorUri + "oob_url");
        var resultContent = await response.Content.ReadAsStringAsync();
        var oob = resultContent.Split("=");
        var oobInvitation = oob[1];

        var decodedInvitation = Encoding.UTF8.GetString(Base64Url.Decode(oobInvitation));
        var oobModel = JsonSerializer.Deserialize<OobModel>(decodedInvitation);
        var invitationPeerDidResult = PeerDidResolver.ResolvePeerDid(new PeerDid(oobModel.From), VerificationMaterialFormatPeerDid.Jwk);
        var invitationPeerDidDocResult = DidDocPeerDid.FromJson(invitationPeerDidResult.Value);

        var secretResolverInMemory = new SecretResolverInMemory();
        var simpleDidDocResolver = new SimpleDidDocResolver();

        var mediatorDid = invitationPeerDidDocResult.Value.Did;
        var mediatorEndpoint = invitationPeerDidDocResult.Value.Services.FirstOrDefault().ServiceEndpoint;

        _createPeerDidHandler = new CreatePeerDidHandler(secretResolverInMemory);

        var localDidOfAliceToUseWithTheMediator = await _createPeerDidHandler.Handle(new CreatePeerDidRequest(), cancellationToken: new CancellationToken());
        var basicMessage = BasicMessage.Create("Hello Mediator", localDidOfAliceToUseWithTheMediator.Value.PeerDid.Value);
        _sendMessageHandler = new SendMessageHandler(_httpClient, new SimpleDidDocResolver(), secretResolverInMemory);
        var abResult = await _sendMessageHandler.Handle(new SendMessageRequest(new Uri(mediatorEndpoint.Uri), mediatorDid, localDidOfAliceToUseWithTheMediator.Value.PeerDid.Value, basicMessage), CancellationToken.None);
        abResult.IsSuccess.Should().BeTrue();
        var content = ((JsonElement)abResult.Value.Body["content"]).GetString();
        content.Should().Be("This is the BLOCKTRUST MEDIATOR answering machine. Thank you for calling! Your message was: 'Hello Mediator'");
    }
}