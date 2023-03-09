namespace Blocktrust.Mediator.Client.BlocktrustIntegrationTests;

using Blocktrust.DIDComm.Secrets;
using Blocktrust.Mediator.Client.Commands.ForwardMessage;
using Blocktrust.Mediator.Client.Commands.MediatorCoordinator.RequestMediation;
using Blocktrust.Mediator.Client.Commands.MediatorCoordinator.UpdateKeys;
using Blocktrust.Mediator.Common;
using Blocktrust.Mediator.Common.Commands.CreatePeerDid;
using Blocktrust.Mediator.Common.Protocols;
using FluentAssertions;
using MediatR;
using Moq;

public class RoutingTests
{
    private readonly Mock<IMediator> _mediatorMock;

    private readonly string _blocktrustMediatorUri = "https://localhost:7037/";
    
    private RequestMediationHandler _requestMediationHandler;
    private readonly HttpClient _httpClient;
    private CreatePeerDidHandler _createPeerDidHandlerAlice;
    private CreatePeerDidHandler _createPeerDidHandlerBob;
    private SendForwardMessageHandler _sendForwardMessageHandler;

    public RoutingTests()
    {
        _mediatorMock = new Mock<IMediator>();
        _httpClient = new HttpClient();
    }

    /// <summary>
    /// This tests assumes that the Blocktrust Mediator is running on https://localhost:7037
    /// </summary>
    [Fact]
    public async Task BobSendsBasicMessageToAlice()
    {
        // First get the OOB from the running mediator
        var response = await _httpClient.GetAsync(_blocktrustMediatorUri + "oob_url");
        var resultContent = await response.Content.ReadAsStringAsync();
        var oob = resultContent.Split("=");
        var oobInvitation = oob[1];

         var secretResolverInMemoryForAlice = new SecretResolverInMemory();
        var simpleDidDocResolverForAlice = new SimpleDidDocResolver();
        _createPeerDidHandlerAlice = new CreatePeerDidHandler(secretResolverInMemoryForAlice);

        var localDidOfAliceToUseWithTheMediator = await _createPeerDidHandlerAlice.Handle(new CreatePeerDidRequest(), cancellationToken: new CancellationToken());
        var request = new RequestMediationRequest(oobInvitation, localDidOfAliceToUseWithTheMediator.Value.PeerDid.Value);

        _requestMediationHandler = new RequestMediationHandler(_httpClient, simpleDidDocResolverForAlice, secretResolverInMemoryForAlice);
        var requestMediationResult = await _requestMediationHandler.Handle(request, CancellationToken.None);

        // Alice create now an additional DID to be used with Bob. Important: The service endpoint of the DID must be set to the mediator endpoint
        var localDidOfAliceToUseWithBob = await _createPeerDidHandlerAlice.Handle(new CreatePeerDidRequest(serviceEndpoint: requestMediationResult.Value.MediatorEndpoint), cancellationToken: new CancellationToken());

        // Alice registers the new DID with the mediator, so the mediator can now accept messages from Bob to Alice
        var updateKeyRequest = new UpdateMediatorKeysRequest(requestMediationResult.Value.MediatorEndpoint, requestMediationResult.Value.MediatorDid, localDidOfAliceToUseWithTheMediator.Value.PeerDid.Value, new List<string>() { localDidOfAliceToUseWithBob.Value.PeerDid.Value }, new List<string>());
        var updateMediatorKeysHandler = new UpdateMediatorKeysHandler(_httpClient, simpleDidDocResolverForAlice, secretResolverInMemoryForAlice);
        var updateKeyResult = await updateMediatorKeysHandler.Handle(updateKeyRequest, CancellationToken.None);
        updateKeyResult.IsSuccess.Should().BeTrue();

        // Bob creates its own DID
        var secretResolverInMemoryForBob = new SecretResolverInMemory();
        var simpleDidDocResolverForBob = new SimpleDidDocResolver();
        _createPeerDidHandlerBob = new CreatePeerDidHandler(secretResolverInMemoryForBob);
        var localDidOfBobToUseWithAlice = await _createPeerDidHandlerBob.Handle(new CreatePeerDidRequest(), cancellationToken: new CancellationToken());

        // Bob creates a "Basic Message" from Bob to Alice (the Did of Alice must be shared with Bob before e.g. with OOB)
        var basicMessage = BasicMessage.Create("Hello Alice");
        var packedBasicMessage = BasicMessage.Pack(basicMessage, from: localDidOfBobToUseWithAlice.Value.PeerDid.Value, localDidOfAliceToUseWithBob.Value.PeerDid.Value, secretResolverInMemoryForBob, simpleDidDocResolverForBob);

        // Bob creates a DID just to be used with the mediator
        var localDidOfBobToUseWithAliceMediator = await _createPeerDidHandlerBob.Handle(new CreatePeerDidRequest(), cancellationToken: new CancellationToken());

        // Wrap the Basic Message into a new Message for the mediator to receive and send it
        _sendForwardMessageHandler = new SendForwardMessageHandler(_mediatorMock.Object, _httpClient, simpleDidDocResolverForBob, secretResolverInMemoryForBob);
        var result = await _sendForwardMessageHandler.Handle(new SendForwardMessageRequest(
            message: packedBasicMessage,
            localDid: localDidOfBobToUseWithAliceMediator.Value.PeerDid.Value,
            mediatorDid: requestMediationResult.Value.MediatorDid, // The mediator DID was also shared beforehand
            mediatorEndpoint: requestMediationResult.Value.MediatorEndpoint,
            recipientDid: localDidOfAliceToUseWithBob.Value.PeerDid.Value
        ), new CancellationToken());
       
        // Assert, that the message was accepted and we get a 202
        result.IsSuccess.Should().BeTrue();
    }

  
}