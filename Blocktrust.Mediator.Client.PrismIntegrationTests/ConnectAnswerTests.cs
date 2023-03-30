namespace Blocktrust.Mediator.Client.PrismIntegrationTests;

using Blocktrust.DIDComm.Secrets;
using Blocktrust.Mediator.Common;
using Blocktrust.Mediator.Common.Commands.CreatePeerDid;
using Commands.MediatorCoordinator.RequestMediation;
using Commands.MediatorCoordinator.UpdateKeys;
using Commands.Pickup.DeliveryRequest;
using Commands.Pickup.MessageReceived;
using Commands.PrismConnect.AnwserConnectRequest;
using Commands.PrismConnect.CreateOobInvitation;
using FluentAssertions;
using MediatR;
using Moq;
using Xunit;

public class ConnectAnswerTests
{
    private readonly HttpClient _httpClient;
    private CreatePeerDidHandler _createPeerDidHandler;
    private readonly DeliveryRequestHandler _deliveryRequestHandler;
    private readonly MessageReceivedHandler _messageReceivedHandler;
    private readonly AnswerPrismConnectHandler _answerPrismConnectHandler;
    private readonly Mock<IMediator> _mediatorMock;
    private readonly SimpleDidDocResolver _simpleDidDocResolver;
    private readonly SecretResolverInMemory _secretResolverInMemory;
    private RequestMediationHandler _requestMediationHandler;

    private readonly string _blocktrustMediatorUri = "http://localhost:5023/";
    private readonly string _prismAgentUrlRunningInDocker = "http://localhost:8090/";
    private readonly string _prismAgentApiKey = "kxr9i@6XgKBUxe%O";


    public ConnectAnswerTests()
    {
        _mediatorMock = new Mock<IMediator>();
        _httpClient = new HttpClient();
        _simpleDidDocResolver = new SimpleDidDocResolver();
        _secretResolverInMemory = new SecretResolverInMemory();
        _deliveryRequestHandler = new DeliveryRequestHandler(_httpClient, _simpleDidDocResolver, _secretResolverInMemory);
        _messageReceivedHandler = new MessageReceivedHandler(_httpClient, _simpleDidDocResolver, _secretResolverInMemory);

        _mediatorMock.Setup(p => p.Send(It.IsAny<DeliveryRequestRequest>(), It.IsAny<CancellationToken>()))
            .Returns(async (DeliveryRequestRequest request, CancellationToken token) => await _deliveryRequestHandler.Handle(request, token));
        _mediatorMock.Setup(p => p.Send(It.IsAny<MessageReceivedRequest>(), It.IsAny<CancellationToken>()))
            .Returns(async (MessageReceivedRequest request, CancellationToken token) => await _messageReceivedHandler.Handle(request, token));

        _answerPrismConnectHandler = new AnswerPrismConnectHandler(_httpClient, _simpleDidDocResolver, _secretResolverInMemory, _mediatorMock.Object);
    }

    /// <summary>
    /// This tests assumes that a PRISM node is running on http://localhost:8080 inside a Docker container and the blocktrust mediator is running on http://localhost:5023
    /// This test does not work with HTTPS!
    /// Also, there is a general issue with the routing of http-request:
    /// The PRISM agent is running inside a Docker container and the blocktrust mediator is running on the host machine.
    /// If I send a message from the host-machine to the docker-container the message cannot be routed back to the host machine, if the host-machine
    /// is referenced by "localhost" - this would cause the PRISM agent to search for a endpoint inside its container.
    /// The message can be routed back if the host-machine is referenced by "host.docker.internal"
    /// The result is, that in the code below sometimes host.docker.internal has to be replaced by localhost and vice versa.
    /// To make this work correctly the mediator has also to use the modified endpoint when it is creating its DIDs.
    /// For this a line of code in the MediatorController was added to handle this special case.
    /// Also we need a new invitation from the PRISM agent. The invitation is expired after a single use.
    /// After the connection is establish we can asked the agent for all its connections and check if the connection is there.
    /// </summary>
    [Fact]
    public async Task ConnectAnswerTestShouldReturnSuccess()
    {
        // The idea here is the exact opposite of the ConnectTest. We create a OOB invitation and send it to the PRISM agent.
        // The PRISM agent will then send a message to our mediator and we have to pick it up and send an answer back to the PRISM agent.

        var initialConnections = await PrismTestHelpers.GetConnections(_prismAgentApiKey, _prismAgentUrlRunningInDocker);
        var initialConnectionsCount = initialConnections.Count;
        
        // First we need a mediator setup for us
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

        // Create a OOB invitation for the PRISM agent
        var (oobInvitationForPrismAgent, messageId) = PrismConnectOobInvitation.Create(localDidToUseWithPrism.Value.PeerDid);

        // Send the OOB invitation to the PRISM agent
        await PrismTestHelpers.SendInvitation(_prismAgentApiKey, _prismAgentUrlRunningInDocker, oobInvitationForPrismAgent);

        // THe message now gets processed by the mediator and we have to pick it up on our agent
        // Since this can take a while we have to wait a few seconds

        var maxWaitTime = new TimeSpan(0,0,0,10);
        var answerPrismConnectRequest = new AnswerPrismConnectRequest(localDidToUseWithPrism.Value.PeerDid, localDidToUseWithMediator.Value.PeerDid.Value, requestMediationResult.Value.MediatorEndpoint, requestMediationResult.Value.MediatorDid, maxWaitTime, messageId);
        var prismConnectResult= await _answerPrismConnectHandler.Handle(answerPrismConnectRequest, CancellationToken.None);

        // Assert
        prismConnectResult.IsSuccess.Should().BeTrue();
        
        // Get the exising connections
        var connections = await PrismTestHelpers.GetConnections(_prismAgentApiKey, _prismAgentUrlRunningInDocker);
        // The new connection-Id should now be added to the list
        // The connectionId is not send back over the wire, so sadly we just have to compare the numbers
        connections.Count.Should().Be(initialConnectionsCount + 1);
    }
}