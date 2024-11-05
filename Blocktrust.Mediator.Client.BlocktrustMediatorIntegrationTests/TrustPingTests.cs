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

namespace Blocktrust.Mediator.Client.BlocktrustIntegrationTests;

public class TrustPingTests
{
    private readonly HttpClient _httpClient;
    private TrustPingHandler _trustPingHandler;
    private CreatePeerDidHandler _createPeerDidHandler;
    private readonly string _blocktrustMediatorUri = "https://localhost:7037/";

    public TrustPingTests()
    {
        _httpClient = new HttpClient();
    }

    /// <summary>
    /// This tests assumes that the Blocktrust Mediator is running on http://127.0.0.1:8000
    /// </summary>
    [Fact]
    public async Task TrustPingTest()
    {
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

        var mediatorDid = invitationPeerDidDocResult.Value.Did;
        var mediatorEndpoint = invitationPeerDidDocResult.Value.Services.FirstOrDefault().ServiceEndpoint;
        
        var secretResolverInMemory = new SecretResolverInMemory();
        _createPeerDidHandler = new CreatePeerDidHandler(secretResolverInMemory);
       
        var localDidOfAliceToUseWithTheMediator = await _createPeerDidHandler.Handle(new CreatePeerDidRequest(), cancellationToken: new CancellationToken());
        var request = new TrustPingRequest(new Uri(mediatorEndpoint.Uri), mediatorDid, localDidOfAliceToUseWithTheMediator.Value.PeerDid.Value, suggestedLabel: "myLabel");

        _trustPingHandler = new TrustPingHandler(_httpClient, new SimpleDidDocResolver(), secretResolverInMemory);
        var trustPingResult = await _trustPingHandler.Handle(request, CancellationToken.None);

        trustPingResult.IsSuccess.Should().BeTrue();
    }
}