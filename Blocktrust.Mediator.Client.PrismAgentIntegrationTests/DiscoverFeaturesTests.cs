namespace Blocktrust.Mediator.Client.PrismIntegrationTests;

using System.Text;
using System.Text.Json;
using Blocktrust.Common.Converter;
using Blocktrust.DIDComm.Secrets;
using Blocktrust.Mediator.Common;
using Blocktrust.Mediator.Common.Commands.CreatePeerDid;
using Blocktrust.Mediator.Common.Models.OutOfBand;
using Blocktrust.PeerDID.DIDDoc;
using Blocktrust.PeerDID.PeerDIDCreateResolve;
using Blocktrust.PeerDID.Types;
using Commands.DiscoverFeatures;
using Commands.MediatorCoordinator.RequestMediation;
using Commands.MediatorCoordinator.UpdateKeys;
using Common.Models.DiscoverFeatures;
using FluentAssertions;
using Xunit;

public class DiscoverFeaturesTests
{
    private readonly HttpClient _httpClient;
    private DiscoverFeaturesHandler _discoverFeaturesHandler;
    private CreatePeerDidHandler _createPeerDidHandler;
    private readonly string _blocktrustMediatorUri = "http://localhost:5023/";
    private readonly string _prismAgentUrlRunningInDocker = "http://localhost:8090/";
    private readonly string _prismAgentApiKey = "kxr9i@6XgKBUxe%O";
    private readonly SimpleDidDocResolver _simpleDidDocResolver;
    private readonly SecretResolverInMemory _secretResolverInMemory;
    private RequestMediationHandler _requestMediationHandler;

    public DiscoverFeaturesTests()
    {
        _httpClient = new HttpClient();
        _simpleDidDocResolver = new SimpleDidDocResolver();
        _secretResolverInMemory = new SecretResolverInMemory();
    }

    /// <summary>
    /// This tests assumes that a PRISM node is running on http://localhost:8080 / 8090 / 9000 inside a Docker container and the blocktrust mediator is running on http://localhost:5023
    /// This test does not work with HTTPS!
    /// </summary>
    [Fact]
    public async Task DiscoverFeaturesShouldSucceed()
    {
        // Not implemented yet as it seems (30.03.2023)
        
        // First get a OOB from the agent, so that we have DID and a endpoint to connect to
        var prismOob = await PrismTestHelpers.RequestOutOfBandInvitation(_prismAgentApiKey, _prismAgentUrlRunningInDocker);
        var decodedInvitationFromPrismAgent = Encoding.UTF8.GetString(Base64Url.Decode(prismOob));
        var oobModelFromPrismAgent = JsonSerializer.Deserialize<OobModel>(decodedInvitationFromPrismAgent);
        var invitationPeerDidResultFromPrismAgent = PeerDidResolver.ResolvePeerDid(new PeerDid(oobModelFromPrismAgent.From), VerificationMaterialFormatPeerDid.Jwk);
        var invitationPeerDidDocResultFromPrismAgent = DidDocPeerDid.FromJson(invitationPeerDidResultFromPrismAgent.Value);
        
        var prismAgentDid = invitationPeerDidDocResultFromPrismAgent.Value.Did;
        var prismAgentEndpoint = invitationPeerDidDocResultFromPrismAgent.Value.Services.FirstOrDefault().ServiceEndpoint;
        prismAgentEndpoint = prismAgentEndpoint.Replace("host.docker.internal", "localhost");
        
        _createPeerDidHandler = new CreatePeerDidHandler(_secretResolverInMemory);
        
        // Then we need a mediator setup for us (so that we can receive the response of the trust ping)
        var response = await _httpClient.GetAsync(_blocktrustMediatorUri + "oob_url");
        var resultContent = await response.Content.ReadAsStringAsync();
        var oob = resultContent.Split("=");
        var oobInvitation = oob[1];
        
        _createPeerDidHandler = new CreatePeerDidHandler(_secretResolverInMemory);
        var localDidToUseWithMediator = await _createPeerDidHandler.Handle(new CreatePeerDidRequest(), cancellationToken: new CancellationToken());
        var requestMediation = new RequestMediationRequest(oobInvitation, localDidToUseWithMediator.Value.PeerDid.Value);
        _requestMediationHandler = new RequestMediationHandler(_httpClient, _simpleDidDocResolver, _secretResolverInMemory);
        var requestMediationResult = await _requestMediationHandler.Handle(requestMediation, CancellationToken.None);
        
        // Create a local DID to use with PRISM Agent
        var localDidToUseWithPrism = await _createPeerDidHandler.Handle(new CreatePeerDidRequest(requestMediationResult.Value.MediatorDid), cancellationToken: new CancellationToken());
        var addKeyRequest = new UpdateMediatorKeysRequest(requestMediationResult.Value.MediatorEndpoint, requestMediationResult.Value.MediatorDid, localDidToUseWithMediator.Value.PeerDid.Value, new List<string>() { localDidToUseWithPrism.Value.PeerDid.Value }, new List<string>());
        var addMediatorKeysHandler = new UpdateMediatorKeysHandler(_httpClient, _simpleDidDocResolver, _secretResolverInMemory);
        await addMediatorKeysHandler.Handle(addKeyRequest, CancellationToken.None);
        
        var queries = new List<FeatureQuery>();
        queries.Add(new FeatureQuery("protocol"));        
        var request = new DiscoverFeaturesRequest(new Uri(prismAgentEndpoint), prismAgentDid, localDidToUseWithPrism.Value.PeerDid.Value, queries);
        _discoverFeaturesHandler = new DiscoverFeaturesHandler(_httpClient, new SimpleDidDocResolver(), _secretResolverInMemory);
        var discoverFeaturesResult = await _discoverFeaturesHandler.Handle(request, CancellationToken.None);
        
        discoverFeaturesResult.IsSuccess.Should().BeTrue();
    }
}