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
using Commands.MediatorCoordinator.RequestMediation;
using Commands.MediatorCoordinator.UpdateKeys;
using Commands.Pickup.DeliveryRequest;
using Commands.Pickup.MessageReceived;
using Commands.PrismConnect.ProcessOobInvitationAndConnect;
using FluentAssertions;
using MediatR;
using Moq;
using Xunit;

public class ConnectTests
{
    private readonly HttpClient _httpClient;
    private PrismConnectHandler _prismConnectHandler;
    private CreatePeerDidHandler _createPeerDidHandler;
    private readonly DeliveryRequestHandler _deliveryRequestHandler;
    private readonly MessageReceivedHandler _messageReceivedHandler;
    private readonly Mock<IMediator> _mediatorMock;
    private readonly SimpleDidDocResolver _simpleDidDocResolver;
    private readonly SecretResolverInMemory _secretResolverInMemory;

    private RequestMediationHandler _requestMediationHandler;

    private readonly string _blocktrustMediatorUri = "http://localhost:5023/";
    private readonly string _prismAgentUrlRunningInDocker = "http://localhost:8090/";
    private readonly string _prismAgentApiKey = "kxr9i@6XgKBUxe%O";



    public ConnectTests()
    {
        _httpClient = new HttpClient();
        _mediatorMock = new Mock<IMediator>();
        _simpleDidDocResolver = new SimpleDidDocResolver();
        _secretResolverInMemory = new SecretResolverInMemory();
        _deliveryRequestHandler = new DeliveryRequestHandler(_httpClient, _simpleDidDocResolver, _secretResolverInMemory);
        _messageReceivedHandler = new MessageReceivedHandler(_httpClient, _simpleDidDocResolver, _secretResolverInMemory);
        
        _mediatorMock.Setup(p => p.Send(It.IsAny<DeliveryRequestRequest>(), It.IsAny<CancellationToken>()))
            .Returns(async (DeliveryRequestRequest request, CancellationToken token) => await _deliveryRequestHandler.Handle(request, token) );
        _mediatorMock.Setup(p => p.Send(It.IsAny<MessageReceivedRequest>(), It.IsAny<CancellationToken>()))
            .Returns(async (MessageReceivedRequest request, CancellationToken token) => await _messageReceivedHandler.Handle(request, token) );
    }

    /// <summary>
    /// This tests assumes that a PRISM node is running on http://localhost:8080 / 8090 / 9000 inside a Docker container and the blocktrust mediator is running on http://localhost:5023
    /// This test does not work with HTTPS!
    /// Also, there is a general issue with the routing of http-request:
    /// The PRISM agent is running inside a Docker container and the blocktrust mediator is running on the host machine.
    /// If I send a message from the host-machine to the docker-container the message cannot be routed back to the host machine, if the host-machine
    /// is referenced by "localhost" - this would cause the PRISM agent to search for a endpoint inside its containter.
    /// The message can be routed back if the host-machine is referenced by "host.docker.internal"
    /// The result is, that in the code below sometimes host.docker.internal has to be replaced by localhost and vice versa.
    /// To make this work correctly the mediator has also to use the modified endpoint when it is creating its DIDs.
    /// For this a line of code in the MediatorController was added to handle this special case.
    /// Also we need a new invitation from the PRISM agent. The invitiation is expired after a single use.
    /// After the connection is establish we can asked the agent for all its connections and check if the connection is there.
    /// </summary>
    [Fact]
    public async Task ConnectTestShouldReturnSuccess()
    {
        // The basic idea here is, that we get a OOB-invitation from PRISM, then send a message to the PRISM agent and the PRISM agent responds
        // If the process is completed we have a fully established connection. We can check that by asking the PRISM agent for all its connections.
        
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

        // Act
        var request = new PrismConnectRequest(
            prismEndpoint: new Uri(prismAgentEndpoint),
            prismDid: prismAgentDid,
            localDidToUseWithPrism: localDidToUseWithPrism.Value.PeerDid.Value,
            threadId: oobModelFromPrismAgent.Id,
            mediatorEndpoint: requestMediationResult.Value.MediatorEndpoint,
            localDidToUseWithMediator: localDidToUseWithMediator.Value.PeerDid.Value,
            mediatorDid: requestMediationResult.Value.MediatorDid);
        _prismConnectHandler = new PrismConnectHandler(_httpClient, new SimpleDidDocResolver(), _secretResolverInMemory, _mediatorMock.Object);
        var prismConnectResult = await _prismConnectHandler.Handle(request, CancellationToken.None);

        // Assert
        prismConnectResult.IsSuccess.Should().BeTrue();
        
        // Get the exising connections
        var connections = await PrismTestHelpers.GetConnections(_prismAgentApiKey, _prismAgentUrlRunningInDocker);
        // The new connection-Id should now be added to the list
        connections.Should().Contain(oobModelFromPrismAgent.Id);
    }
}