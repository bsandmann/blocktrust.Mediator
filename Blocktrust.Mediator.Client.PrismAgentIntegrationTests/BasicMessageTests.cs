namespace Blocktrust.Mediator.Client.PrismIntegrationTests;

using System.Text;
using System.Text.Json;
using Blocktrust.Common.Converter;
using Blocktrust.Common.Models.DidDoc;
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
using Commands.SendMessage;
using Common.Models.DiscoverFeatures;
using Common.Protocols;
using FluentAssertions;
using Xunit;

public class BasicMessageTests
{
    // http://212.124.51.147:35412/cloud-agent/
    private readonly HttpClient _httpClient;
    private DiscoverFeaturesHandler _discoverFeaturesHandler;
    private CreatePeerDidHandler _createPeerDidHandler;
    private readonly string _blocktrustMediatorUri = "https://localhost:7037/";
    private readonly string _prismAgentUrlRunningInDocker = "http://212.124.51.147:35412/";
    private readonly string _prismAgentApiKey = "1623db3e7de4a24c";
    private readonly SimpleDidDocResolver _simpleDidDocResolver;
    private readonly SecretResolverInMemory _secretResolverInMemory;
    private RequestMediationHandler _requestMediationHandler;

    public BasicMessageTests()
    {
        _httpClient = new HttpClient();
        _simpleDidDocResolver = new SimpleDidDocResolver();
        _secretResolverInMemory = new SecretResolverInMemory();
    }

    /// <summary>
    /// This tests assumes that a PRISM node is running on https://beta-mediator.atalaprism.io
    /// </summary>
    [Fact]
    public async Task BasicMessageShouldSucceed()
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
        prismAgentEndpoint = new ServiceEndpoint(uri: prismAgentEndpoint.Uri.Replace("host.docker.internal", "localhost"));
        
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
        
        // Create a simple BasicMessage
        var basicMessage = BasicMessage.Create("Hello agent", localDidToUseWithPrism.Value.PeerDid.Value);

        var sendMessageHandler = new SendMessageHandler(_httpClient,_simpleDidDocResolver, _secretResolverInMemory);
        var sendMessageRequest = new SendMessageRequest(new Uri(prismAgentEndpoint.Uri), prismAgentDid, localDidToUseWithPrism.Value.PeerDid.Value, basicMessage);
        var sendMessageResult = await sendMessageHandler.Handle(sendMessageRequest, CancellationToken.None);
        
        sendMessageResult.IsSuccess.Should().BeTrue();
    }
}