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
using Commands.CredentialRequest;
using Commands.MediatorCoordinator.RequestMediation;
using Commands.MediatorCoordinator.UpdateKeys;
using Commands.Pickup.DeliveryRequest;
using Commands.Pickup.MessageReceived;
using Commands.PrismConnect.AnwserConnectRequest;
using Commands.PrismConnect.ProcessOobInvitationAndConnect;
using Common.Models.CredentialOffer;
using FluentAssertions;
using MediatR;
using Moq;
using Xunit;

public class OfferCredentialTests
{
    private readonly HttpClient _httpClient;
    private PrismConnectHandler _prismConnectHandler;
    private CreatePeerDidHandler _createPeerDidHandler;
    private DeliveryRequestHandler _deliveryRequestHandler;
    private readonly MessageReceivedHandler _messageReceivedHandler;
    private readonly CredentialRequestHandler _credentialRequestHandler;
    private readonly Mock<IMediator> _mediatorMock;
    private readonly SimpleDidDocResolver _simpleDidDocResolver;
    private readonly SecretResolverInMemory _secretResolverInMemory;

    private RequestMediationHandler _requestMediationHandler;

    private readonly string _blocktrustMediatorUri = "http://localhost:5023/";
    private readonly string _prismAgentUrlRunningInDocker = "http://localhost:8090/";
    private readonly string _prismAgentApiKey = "kxr9i@6XgKBUxe%O";


    public OfferCredentialTests()
    {
        _httpClient = new HttpClient();
        _mediatorMock = new Mock<IMediator>();
        _simpleDidDocResolver = new SimpleDidDocResolver();
        _secretResolverInMemory = new SecretResolverInMemory();
        _deliveryRequestHandler = new DeliveryRequestHandler(_httpClient, _simpleDidDocResolver, _secretResolverInMemory);
        _credentialRequestHandler = new CredentialRequestHandler(_httpClient, _simpleDidDocResolver, _secretResolverInMemory);
        _messageReceivedHandler = new MessageReceivedHandler(_httpClient, _simpleDidDocResolver, _secretResolverInMemory);

        _mediatorMock.Setup(p => p.Send(It.IsAny<DeliveryRequestRequest>(), It.IsAny<CancellationToken>()))
            .Returns(async (DeliveryRequestRequest request, CancellationToken token) => await _deliveryRequestHandler.Handle(request, token));
        _mediatorMock.Setup(p => p.Send(It.IsAny<MessageReceivedRequest>(), It.IsAny<CancellationToken>()))
            .Returns(async (MessageReceivedRequest request, CancellationToken token) => await _messageReceivedHandler.Handle(request, token));
    }

    /// <summary>
    /// This tests assumes that a PRISM node is running on http://localhost:8080 / 8090 / 9000 inside a Docker container and the blocktrust mediator is running on http://localhost:5023
    /// The test first runs the connection test
    /// </summary>
    [Fact]
    public async Task ConnectAndGetCredentialOffer()
    {
        // The basic idea here is, that we get a OOB-invitation from PRISM, then send a message to the PRISM agent and the PRISM agent responds
        // If the process is completed we have a fully established connection. We can check that by asking the PRISM agent for all its connections.

        /////////// SETUP THE CONNECTION (as in the ConnectTest)

        // Get and parse the OOB
        var prismOob = await PrismTestHelpers.RequestOutOfBandInvitation(_prismAgentApiKey, _prismAgentUrlRunningInDocker);
        var decodedInvitationFromPrismAgent = Encoding.UTF8.GetString(Base64Url.Decode(prismOob));
        var oobModelFromPrismAgent = JsonSerializer.Deserialize<OobModel>(decodedInvitationFromPrismAgent);
        var invitationPeerDidResultFromPrismAgent = PeerDidResolver.ResolvePeerDid(new PeerDid(oobModelFromPrismAgent.From), VerificationMaterialFormatPeerDid.Jwk);
        var invitationPeerDidDocResultFromPrismAgent = DidDocPeerDid.FromJson(invitationPeerDidResultFromPrismAgent.Value);

        var prismAgentDid = invitationPeerDidDocResultFromPrismAgent.Value.Did;
        var prismAgentEndpoint = invitationPeerDidDocResultFromPrismAgent.Value.Services.FirstOrDefault().ServiceEndpoint;
        prismAgentEndpoint = prismAgentEndpoint.Replace("host.docker.internal", "localhost");

        // Setup Mediator
        var response = await _httpClient.GetAsync(_blocktrustMediatorUri + "oob_url");
        var resultContent = await response.Content.ReadAsStringAsync();
        var oob = resultContent.Split("=");
        var oobInvitation = oob[1];

        _createPeerDidHandler = new CreatePeerDidHandler(_secretResolverInMemory);
        var localDidToUseWithMediator = await _createPeerDidHandler.Handle(new CreatePeerDidRequest(), cancellationToken: new CancellationToken());
        var requestMediation = new RequestMediationRequest(oobInvitation, localDidToUseWithMediator.Value.PeerDid.Value);
        _requestMediationHandler = new RequestMediationHandler(_httpClient, _simpleDidDocResolver, _secretResolverInMemory);
        var requestMediationResult = await _requestMediationHandler.Handle(requestMediation, CancellationToken.None);

        // Create a DID to use with PRISM Agent
        var localDidToUseWithPrism = await _createPeerDidHandler.Handle(new CreatePeerDidRequest(requestMediationResult.Value.MediatorDid), cancellationToken: new CancellationToken());
        var addKeyRequest = new UpdateMediatorKeysRequest(requestMediationResult.Value.MediatorEndpoint, requestMediationResult.Value.MediatorDid, localDidToUseWithMediator.Value.PeerDid.Value, new List<string>() { localDidToUseWithPrism.Value.PeerDid.Value }, new List<string>());
        var addMediatorKeysHandler = new UpdateMediatorKeysHandler(_httpClient, _simpleDidDocResolver, _secretResolverInMemory);
        await addMediatorKeysHandler.Handle(addKeyRequest, CancellationToken.None);

        // Request a connection
        var request = new PrismConnectRequest(
            prismEndpoint: prismAgentEndpoint,
            prismDid: prismAgentDid,
            localDidToUseWithPrism: localDidToUseWithPrism.Value.PeerDid.Value,
            threadId: oobModelFromPrismAgent.Id,
            mediatorEndpoint: requestMediationResult.Value.MediatorEndpoint,
            localDidToUseWithMediator: localDidToUseWithMediator.Value.PeerDid.Value,
            mediatorDid: requestMediationResult.Value.MediatorDid);
        _prismConnectHandler = new PrismConnectHandler(_httpClient, new SimpleDidDocResolver(), _secretResolverInMemory, _mediatorMock.Object);
        var prismConnectResult = await _prismConnectHandler.Handle(request, CancellationToken.None);

        // Assert that the connection was successful
        prismConnectResult.IsSuccess.Should().BeTrue();

        // Get the exising connections on the PRISM agent
        var connections = await PrismTestHelpers.GetConnections(_prismAgentApiKey, _prismAgentUrlRunningInDocker);
        // The new connection-Id should now be added to the list
        connections.Should().Contain(oobModelFromPrismAgent.Id);
        var connectionId = oobModelFromPrismAgent.Id;

        ////////// NOW WE HAVE TO CREATE A PUBLISHED DID ON THE PRISM AGENT
        var unpublishedDid = await PrismTestHelpers.CreateUnpublishedDid(_prismAgentApiKey, _prismAgentUrlRunningInDocker);
        var publishedDidOperations = await PrismTestHelpers.PublishDid(_prismAgentApiKey, _prismAgentUrlRunningInDocker, unpublishedDid);
        var didPublishingState = string.Empty;
        do
        {
            // WE HAVE TO WAIT HERE FOR THE DID TO BE PUBLISHED
            await Task.Delay(1000);
            didPublishingState = await PrismTestHelpers.GetDid(_prismAgentApiKey, _prismAgentUrlRunningInDocker, unpublishedDid);
        } while (!didPublishingState.Equals("PUBLISHED", StringComparison.InvariantCultureIgnoreCase));

        ////////// NOW WE HAVE TO CREATE A CREDENTIAL OFFER ON THE PRISM AGENT
        var isPending = await PrismTestHelpers.CreateCredentialOffer(_prismAgentApiKey, _prismAgentUrlRunningInDocker, unpublishedDid, connectionId);
        isPending.Should().Be(true);

        ///////// Get to the Mediator and hope the our message is there. Parse it
        await Task.Delay(2000);
        _deliveryRequestHandler = new DeliveryRequestHandler(_httpClient, _simpleDidDocResolver, _secretResolverInMemory);
        var deliveryRequestForOfferResult = await _deliveryRequestHandler.Handle(new DeliveryRequestRequest(localDidToUseWithMediator.Value.PeerDid.Value, requestMediationResult.Value.MediatorDid, requestMediationResult.Value.MediatorEndpoint, 100), new CancellationToken());
        deliveryRequestForOfferResult.Value.Messages.Count.Should().Be(1);
        var credentialOffer = CredentialOffer.ParseCredentialOffer(deliveryRequestForOfferResult.Value.Messages.First());
        credentialOffer.IsSuccess.Should().BeTrue();

        /////// Send a response to the agent 
        // The problem is that we have to provide a signed JWT here, which contains a challenge
        // By providing this string here I'm able to get the agent to accept the credential offer
        // But it won't process it further, because the challenge is not correct
        var signedJwtCredentialRequest = "ZXlKaGJHY2lPaUpGVXpJMU5rc2lmUS5leUpwYzNNaU9pSmthV1E2Y0hKcGMyMDZPVGxtTjJReE16Vm1aVEk0WlRVM09EbG1Oems0T0dRME5EVTBPVGMwWmpabU5UTTBNVEZpT0RNNE9EWmlPR0psTldaaU5ERmtNVFkzTm1FNVlqQTBPVHBEY3pCRFEzTnZRMFZzYTB0Q1IzUnNaVlJGVVVKRlNsQkRaMng2V2xkT2QwMXFWVEpoZWtWVFNVZzVjMWRWUm1aSloxaDBiM3BvTUZsWWFVeFRMVmRZUW5oTFUzbE9VMUZPVWtRelQwdFJlbUU0YkdGSGFVSlFkR2M1ZFRVeE5XdE5XV1JDY0dOWmQyOHhhVzUwVFhGM05FRnRSRlJKYVhKT09UQlRRMHQyV25aU1NscERaMUp5V2xocmVVVkJTa05VZDI5S1l6SldhbU5FU1RGT2JYTjRSV2xFZG5SdldsVkhNbHBQWWtJNVZsQnNVMFk0TWxCNGNISmxZMmRQWjJ3NU5IaDRaVkJRYldrMmIxcFZhRzluTVdWUlRsTk1WbGRGV0VaNlkyMVJUV1JUZFZRM1VqVk9NVlpvV2psdWJURnZYMjh0U0RoYU1XUnFPRk5ZUVc5SVlsZEdlbVJIVm5sTlFrRkNVV3M0UzBOWVRteFpNMEY1VGxSYWNrMVNTV2RITlZvd1VDMTNjMnBaTkdkaExVbEtWVU54YlZKTWRqUkxjSE5FVDJOV2JGZHhRVk5ZTkZGcWJWaHpZVWxHY2tsWVJFZGZXRzFOVG1GbE1sbENjMkl4ZEdob05sOVdkVjlFZFhKWFdrY3lWa2hPVDBKaVpHNTRSMnBSUzBReVVuQmFSSEIzWTIxc2VtSlVjREJhV0U0d1RWSkpUbFJIYkhWaE1sWnJVa2M1ZEZsWGJIVmplRzlUWVVoU01HTklUVFpNZVRrd1dsaE9NRTFUTldwaU1qQjJJaXdpWVhWa0lqb2laRzl0WVdsdUlpd2lkbkFpT25zaWRIbHdaU0k2V3lKV1pYSnBabWxoWW14bFVISmxjMlZ1ZEdGMGFXOXVJbDBzSWtCamIyNTBaWGgwSWpwYkltaDBkSEJ6T2x3dlhDOTNkM2N1ZHpNdWIzSm5YQzh5TURFNFhDOXdjbVZ6Wlc1MFlYUnBiMjV6WEM5Mk1TSmRmU3dpYm05dVkyVWlPaUk1WXpJNE1UWXlZaTFpWVdVMExUUTBNbUl0WWpWaU9DMWlOelkxTUdZek0ySmpPVE1pZlEuX1BnY05nb0tuN1JsTmlQVTdudXFfRVVJaWR1dWFNSXZqeklmN0RJMGpiWUNtSGl3cTBTWGVMNVBLQmptODBwNlhMYW00cFh6enZIdjhCajB2Tkdud0E=";
        var credentialRequest = new CredentialRequestRequest(messageId: credentialOffer.Value.MessageId, localPeerDid: credentialOffer.Value.To, prismPeerDid: credentialOffer.Value.From, signedJwtCredentialRequest);
        var credentialRequestResult = await _credentialRequestHandler.Handle(credentialRequest, CancellationToken.None);
        credentialRequestResult.IsSuccess.Should().BeTrue();
        
        // /////// Get the credential from the mediator
        // cannot be implemented without having real PRISM DIDs with private keys as well as crypto functions here
    }
}