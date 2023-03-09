namespace Blocktrust.Mediator.Client.RootsIntegrationTests;

using Blocktrust.Mediator.Common.Commands.CreatePeerDid;
using Commands.ForwardMessage;
using Commands.MediatorCoordinator.RequestMediation;
using Commands.MediatorCoordinator.UpdateKeys;
using Common;
using Common.Protocols;
using DIDComm.Secrets;
using FluentAssertions;
using MediatR;
using Moq;
using Xunit;

public class RoutingTests
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly HttpClient _httpClient;
    private RequestMediationHandler _requestMediationHandler;
    private CreatePeerDidHandler _createPeerDidHandlerAlice;
    private CreatePeerDidHandler _createPeerDidHandlerBob;
    private SendForwardMessageHandler _sendForwardMessageHandler;

    public RoutingTests()
    {
        _mediatorMock = new Mock<IMediator>();
        _httpClient = new HttpClient();
    }

    /// <summary>
    /// This tests assumes that the Roots Mediator is running on http://127.0.0.1:8000
    /// </summary>
    [Fact]
    public async Task BobSendsBasicMessageToAlice()
    {
        // Setup a Did and a mediator for Alice
        var oobInvitationRootsLocal =
            "eyJ0eXBlIjoiaHR0cHM6Ly9kaWRjb21tLm9yZy9vdXQtb2YtYmFuZC8yLjAvaW52aXRhdGlvbiIsImlkIjoiNGZjN2Q3NDYtMzk2Ny00NjFjLTg5MTAtMWM5YTBmMjdkYjQ0IiwiZnJvbSI6ImRpZDpwZWVyOjIuRXo2TFNkRWNzVnZ6ZTNjWkpxaTFLRFdyU2N5MmFINW9IOFlkUVJRaTZmNVpIN1lGMi5WejZNa284aXllUDRITTFpS3ZuY2o3TkVRQ3JjeXpQN1Y1VW1ad2N1QXN6NWhZRkJlLlNleUpwWkNJNkltNWxkeTFwWkNJc0luUWlPaUprYlNJc0luTWlPaUpvZEhSd09pOHZNVEkzTGpBdU1DNHhPamd3TURBaUxDSmhJanBiSW1ScFpHTnZiVzB2ZGpJaVhYMCIsImJvZHkiOnsiZ29hbF9jb2RlIjoicmVxdWVzdC1tZWRpYXRlIiwiZ29hbCI6IlJlcXVlc3RNZWRpYXRlIiwibGFiZWwiOiJNZWRpYXRvciIsImFjY2VwdCI6WyJkaWRjb21tL3YyIl19fQ";

        var secretResolverInMemoryForAlice = new SecretResolverInMemory();
        var simpleDidDocResolverForAlice = new SimpleDidDocResolver();
        _createPeerDidHandlerAlice = new CreatePeerDidHandler(secretResolverInMemoryForAlice);

        var localDidOfAliceToUseWithTheMediator = await _createPeerDidHandlerAlice.Handle(new CreatePeerDidRequest(), cancellationToken: new CancellationToken());
        var request = new RequestMediationRequest(oobInvitationRootsLocal, localDidOfAliceToUseWithTheMediator.Value.PeerDid.Value);

        _requestMediationHandler = new RequestMediationHandler(_httpClient, simpleDidDocResolverForAlice, secretResolverInMemoryForAlice);
        var requestMediationResult = await _requestMediationHandler.Handle(request, CancellationToken.None);

        // Alice create now an additional DID to be used with Bob. Important: The service endpoint of the DID must be set to the mediator endpoint
        var localDidOfAliceToUseWithBob = await _createPeerDidHandlerAlice.Handle(new CreatePeerDidRequest(serviceEndpoint: requestMediationResult.Value.MediatorEndpoint), cancellationToken: new CancellationToken());

        // Alice registers the new DID with the mediator, so the mediator can now accept messages from Bob to Alice
        var addKeyRequest = new UpdateMediatorKeysRequest(requestMediationResult.Value.MediatorEndpoint, requestMediationResult.Value.MediatorDid, localDidOfAliceToUseWithTheMediator.Value.PeerDid.Value, new List<string>() { localDidOfAliceToUseWithBob.Value.PeerDid.Value }, new List<string>());
        var addMediatorKeysHandler = new UpdateMediatorKeysHandler(_httpClient, simpleDidDocResolverForAlice, secretResolverInMemoryForAlice);
        var addKeyResult = await addMediatorKeysHandler.Handle(addKeyRequest, CancellationToken.None);
        addKeyResult.IsSuccess.Should().BeTrue();

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
        _sendForwardMessageHandler = new SendForwardMessageHandler(_httpClient, simpleDidDocResolverForBob, secretResolverInMemoryForBob);
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