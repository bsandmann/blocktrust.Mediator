namespace Blocktrust.Mediator.Client.PrismMediatorIntegrationTests;

using System.Text;
using System.Text.Json;
using Blocktrust.Common.Converter;
using Blocktrust.DIDComm.Secrets;
using Blocktrust.Mediator.Client.Commands.TrustPing;
using Blocktrust.Mediator.Common;
using Blocktrust.Mediator.Common.Commands.CreatePeerDid;
using Blocktrust.Mediator.Common.Models.OutOfBand;
using Blocktrust.PeerDID.DIDDoc;
using Blocktrust.PeerDID.PeerDIDCreateResolve;
using Blocktrust.PeerDID.Types;
using FluentAssertions;
using Xunit;

public class TrustPingTests
{
    private readonly HttpClient _httpClient;
    private TrustPingHandler _trustPingHandler;
    private CreatePeerDidHandler _createPeerDidHandler;
    private readonly string _prismMediatorUri = "https://sandbox-mediator.atalaprism.io/";

    public TrustPingTests()
    {
        _httpClient = new HttpClient();
    }

    /// <summary>
    /// This tests assumes that the Roots Mediator is running on https://beta-mediator.atalaprism.io
    /// </summary>
    [Fact]
    public async Task TrustPingTest()
    {
        // Arrange
        // First get the OOB from the running mediator
        // var response = await _httpClient.GetAsync(_prismMediatorUri+ "invitationOOB");
        // var resultContent = await response.Content.ReadAsStringAsync();
        // var oob = resultContent.Split("=");
        // var oobInvitation = oob[1];
        var oobInvitation =
            "eyJpZCI6IjQxN2EzODIyLTM3ZmQtNDhiYy1hMGM0LTBiZTVmYWY0M2IyMyIsInR5cGUiOiJodHRwczovL2RpZGNvbW0ub3JnL291dC1vZi1iYW5kLzIuMC9pbnZpdGF0aW9uIiwiZnJvbSI6ImRpZDpwZWVyOjIuRXo2TFNnaHdTRTQzN3duREUxcHQzWDZoVkRVUXpTanNIemlucFgzWEZ2TWpSQW03eS5WejZNa2hoMWU1Q0VZWXE2SkJVY1RaNkNwMnJhbkNXUnJ2N1lheDNMZTRONTlSNmRkLlNleUowSWpvaVpHMGlMQ0p6SWpwN0luVnlhU0k2SW1oMGRIQnpPaTh2YzJGdVpHSnZlQzF0WldScFlYUnZjaTVoZEdGc1lYQnlhWE50TG1sdklpd2lZU0k2V3lKa2FXUmpiMjF0TDNZeUlsMTlmUS5TZXlKMElqb2laRzBpTENKeklqcDdJblZ5YVNJNkluZHpjem92TDNOaGJtUmliM2d0YldWa2FXRjBiM0l1WVhSaGJHRndjbWx6YlM1cGJ5OTNjeUlzSW1FaU9sc2laR2xrWTI5dGJTOTJNaUpkZlgwIiwiYm9keSI6eyJnb2FsX2NvZGUiOiJyZXF1ZXN0LW1lZGlhdGUiLCJnb2FsIjoiUmVxdWVzdE1lZGlhdGUiLCJhY2NlcHQiOlsiZGlkY29tbS92MiJdfSwidHlwIjoiYXBwbGljYXRpb24vZGlkY29tbS1wbGFpbitqc29uIn0";
        var decodedInvitation = Encoding.UTF8.GetString(Base64Url.Decode(oobInvitation));
        var oobModel = JsonSerializer.Deserialize<OobModel>(decodedInvitation);
        var invitationPeerDidResult = PeerDidResolver.ResolvePeerDid(new PeerDid(oobModel.From), VerificationMaterialFormatPeerDid.Jwk);
        var invitationPeerDidDocResult = DidDocPeerDid.FromJson(invitationPeerDidResult.Value);

        var mediatorDid = invitationPeerDidDocResult.Value.Did;
        var mediatorEndpoint = invitationPeerDidDocResult.Value.Services.FirstOrDefault().ServiceEndpoint;

        var secretResolverInMemory = new SecretResolverInMemory();
        _createPeerDidHandler = new CreatePeerDidHandler(secretResolverInMemory);

        var localDidOfAliceToUseWithTheMediator = await _createPeerDidHandler.Handle(new CreatePeerDidRequest(), cancellationToken: new CancellationToken());
        var request = new TrustPingRequest(new Uri(mediatorEndpoint.Uri), mediatorDid, localDidOfAliceToUseWithTheMediator.Value.PeerDid.Value, true, suggestedLabel: "myLabel");

        _trustPingHandler = new TrustPingHandler(_httpClient, new SimpleDidDocResolver(), secretResolverInMemory);
        var trustPingResult = await _trustPingHandler.Handle(request, CancellationToken.None);

        trustPingResult.IsSuccess.Should().BeTrue();
    }
}