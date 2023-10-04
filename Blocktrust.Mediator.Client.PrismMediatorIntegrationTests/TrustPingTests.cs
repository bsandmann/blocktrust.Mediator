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
    private readonly string _prismMediatorUri  = "https://beta-mediator.atalaprism.io/";

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
        var response = await _httpClient.GetAsync(_prismMediatorUri+ "invitationOOB");
        var resultContent = await response.Content.ReadAsStringAsync();
        var oob = resultContent.Split("=");
        var oobInvitation = oob[1];
        
        var decodedInvitation = Encoding.UTF8.GetString(Base64Url.Decode(oobInvitation));
        var oobModel = JsonSerializer.Deserialize<OobModel>(decodedInvitation);
        var invitationPeerDidResult = PeerDidResolver.ResolvePeerDid(new PeerDid(oobModel.From), VerificationMaterialFormatPeerDid.Jwk);
        var invitationPeerDidDocResult = DidDocPeerDid.FromJson(invitationPeerDidResult.Value);

        var mediatorDid = invitationPeerDidDocResult.Value.Did;
        var mediatorEndpoint = invitationPeerDidDocResult.Value.Services.FirstOrDefault().ServiceEndpoint;
        
        var secretResolverInMemory = new SecretResolverInMemory();
        _createPeerDidHandler = new CreatePeerDidHandler(secretResolverInMemory);
       
        var localDidOfAliceToUseWithTheMediator = await _createPeerDidHandler.Handle(new CreatePeerDidRequest(), cancellationToken: new CancellationToken());
        var request = new TrustPingRequest(new Uri(mediatorEndpoint), mediatorDid, localDidOfAliceToUseWithTheMediator.Value.PeerDid.Value, true, suggestedLabel: "myLabel");

        _trustPingHandler = new TrustPingHandler(_httpClient, new SimpleDidDocResolver(), secretResolverInMemory);
        var trustPingResult = await _trustPingHandler.Handle(request, CancellationToken.None);

        trustPingResult.IsSuccess.Should().BeTrue();
    }
}