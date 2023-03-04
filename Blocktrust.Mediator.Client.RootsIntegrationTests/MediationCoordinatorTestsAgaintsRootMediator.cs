namespace Blocktrust.Mediator.Client.RootsIntegrationTests;

using Blocktrust.Common.Resolver;
using Blocktrust.Mediator.Common.Commands.CreatePeerDid;
using Commands.MediatorCoordinator.QueryKeys;
using Commands.MediatorCoordinator.RequestMediation;
using Commands.MediatorCoordinator.UpdateKeys;
using Common;
using DIDComm.Secrets;
using FluentAssertions;
using FluentResults;
using MediatR;
using Moq;
using PeerDID.PeerDIDCreateResolve;
using Xunit;

public class MediationCoordinatorTestsAgainstBlocktrustMediator
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly HttpClient _httpClient;
    private RequestMediationHandler _requestMediationHandler;
    private CreatePeerDidHandler _createPeerDidHandler;

    public MediationCoordinatorTestsAgainstBlocktrustMediator()
    {
        _mediatorMock = new Mock<IMediator>();
        _httpClient = new HttpClient();
    }

    /// <summary>
    /// This tests assumes that the Blocktrust Mediator is running on http:/localhost:7037
    /// </summary>
    [Fact]
    public async Task InitiateMediateRequestsGetsGranted()
    {
        // Arrange
        var oobInvitationRootsLocal =
            "eyJ0eXBlIjoiaHR0cHM6Ly9kaWRjb21tLm9yZy9vdXQtb2YtYmFuZC8yLjAvaW52aXRhdGlvbiIsImlkIjoiNGZjN2Q3NDYtMzk2Ny00NjFjLTg5MTAtMWM5YTBmMjdkYjQ0IiwiZnJvbSI6ImRpZDpwZWVyOjIuRXo2TFNkRWNzVnZ6ZTNjWkpxaTFLRFdyU2N5MmFINW9IOFlkUVJRaTZmNVpIN1lGMi5WejZNa284aXllUDRITTFpS3ZuY2o3TkVRQ3JjeXpQN1Y1VW1ad2N1QXN6NWhZRkJlLlNleUpwWkNJNkltNWxkeTFwWkNJc0luUWlPaUprYlNJc0luTWlPaUpvZEhSd09pOHZNVEkzTGpBdU1DNHhPamd3TURBaUxDSmhJanBiSW1ScFpHTnZiVzB2ZGpJaVhYMCIsImJvZHkiOnsiZ29hbF9jb2RlIjoicmVxdWVzdC1tZWRpYXRlIiwiZ29hbCI6IlJlcXVlc3RNZWRpYXRlIiwibGFiZWwiOiJNZWRpYXRvciIsImFjY2VwdCI6WyJkaWRjb21tL3YyIl19fQ";

        var secretResolverInMemory = new SecretResolverInMemory();
        _createPeerDidHandler = new CreatePeerDidHandler(secretResolverInMemory);

        var localDid = await _createPeerDidHandler.Handle(new CreatePeerDidRequest(), cancellationToken: new CancellationToken());
        var request = new RequestMediationRequest(oobInvitationRootsLocal, localDid.Value.PeerDid.Value);

        // Act
        _requestMediationHandler = new RequestMediationHandler(_mediatorMock.Object, _httpClient, new SimpleDidDocResolver(), secretResolverInMemory);
        var result = await _requestMediationHandler.Handle(request, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.MediationGranted.Should().BeTrue();
        result.Value.RoutingDid.Should().NotBeNullOrEmpty();
    }


    [Fact]
    public async Task InitiateMediateRequestsGetsDeniedTheSecondTimeBecauseOfExistingConnection()
    {
        // Arrange
        var oobInvitationRootsLocal =
            "eyJ0eXBlIjoiaHR0cHM6Ly9kaWRjb21tLm9yZy9vdXQtb2YtYmFuZC8yLjAvaW52aXRhdGlvbiIsImlkIjoiNGZjN2Q3NDYtMzk2Ny00NjFjLTg5MTAtMWM5YTBmMjdkYjQ0IiwiZnJvbSI6ImRpZDpwZWVyOjIuRXo2TFNkRWNzVnZ6ZTNjWkpxaTFLRFdyU2N5MmFINW9IOFlkUVJRaTZmNVpIN1lGMi5WejZNa284aXllUDRITTFpS3ZuY2o3TkVRQ3JjeXpQN1Y1VW1ad2N1QXN6NWhZRkJlLlNleUpwWkNJNkltNWxkeTFwWkNJc0luUWlPaUprYlNJc0luTWlPaUpvZEhSd09pOHZNVEkzTGpBdU1DNHhPamd3TURBaUxDSmhJanBiSW1ScFpHTnZiVzB2ZGpJaVhYMCIsImJvZHkiOnsiZ29hbF9jb2RlIjoicmVxdWVzdC1tZWRpYXRlIiwiZ29hbCI6IlJlcXVlc3RNZWRpYXRlIiwibGFiZWwiOiJNZWRpYXRvciIsImFjY2VwdCI6WyJkaWRjb21tL3YyIl19fQ";

        var secretResolverInMemory = new SecretResolverInMemory();
        _createPeerDidHandler = new CreatePeerDidHandler(secretResolverInMemory);

        var localDid = await _createPeerDidHandler.Handle(new CreatePeerDidRequest(), cancellationToken: new CancellationToken());
        var firstRequest = new RequestMediationRequest(oobInvitationRootsLocal, localDid.Value.PeerDid.Value);

        _requestMediationHandler = new RequestMediationHandler(_mediatorMock.Object, _httpClient, new SimpleDidDocResolver(), secretResolverInMemory);
        var firstResult = await _requestMediationHandler.Handle(firstRequest, CancellationToken.None);
        firstResult.IsSuccess.Should().BeTrue();
        firstResult.Value.MediationGranted.Should().BeTrue();
        firstResult.Value.RoutingDid.Should().NotBeNullOrEmpty();

        // Act
        var secondRequest = new RequestMediationRequest(oobInvitationRootsLocal, localDid.Value.PeerDid.Value);
        var secondResult = await _requestMediationHandler.Handle(secondRequest, CancellationToken.None);

        // Assert
        secondResult.IsSuccess.Should().BeTrue();
        secondResult.Value.MediationGranted.Should().BeFalse();
    }

    [Fact]
    public async Task AddKeyToExistingConnection()
    {
        // Arrange
        var oobInvitationRootsLocal =
            "eyJ0eXBlIjoiaHR0cHM6Ly9kaWRjb21tLm9yZy9vdXQtb2YtYmFuZC8yLjAvaW52aXRhdGlvbiIsImlkIjoiNGZjN2Q3NDYtMzk2Ny00NjFjLTg5MTAtMWM5YTBmMjdkYjQ0IiwiZnJvbSI6ImRpZDpwZWVyOjIuRXo2TFNkRWNzVnZ6ZTNjWkpxaTFLRFdyU2N5MmFINW9IOFlkUVJRaTZmNVpIN1lGMi5WejZNa284aXllUDRITTFpS3ZuY2o3TkVRQ3JjeXpQN1Y1VW1ad2N1QXN6NWhZRkJlLlNleUpwWkNJNkltNWxkeTFwWkNJc0luUWlPaUprYlNJc0luTWlPaUpvZEhSd09pOHZNVEkzTGpBdU1DNHhPamd3TURBaUxDSmhJanBiSW1ScFpHTnZiVzB2ZGpJaVhYMCIsImJvZHkiOnsiZ29hbF9jb2RlIjoicmVxdWVzdC1tZWRpYXRlIiwiZ29hbCI6IlJlcXVlc3RNZWRpYXRlIiwibGFiZWwiOiJNZWRpYXRvciIsImFjY2VwdCI6WyJkaWRjb21tL3YyIl19fQ";

        var secretResolverInMemory = new SecretResolverInMemory();
        var simpleDidDocResolver = new SimpleDidDocResolver();
        _createPeerDidHandler = new CreatePeerDidHandler(secretResolverInMemory);

        var localDid = await _createPeerDidHandler.Handle(new CreatePeerDidRequest(), cancellationToken: new CancellationToken());
        var request = new RequestMediationRequest(oobInvitationRootsLocal, localDid.Value.PeerDid.Value);

        _requestMediationHandler = new RequestMediationHandler(_mediatorMock.Object, _httpClient, simpleDidDocResolver, secretResolverInMemory);
        var mediationResult = await _requestMediationHandler.Handle(request, CancellationToken.None);

        mediationResult.IsSuccess.Should().BeTrue();
        mediationResult.Value.MediationGranted.Should().BeTrue();

        // Act
        var someTestKeysToAdd = await _createPeerDidHandler.Handle(new CreatePeerDidRequest(), cancellationToken: new CancellationToken());
        var addKeyRequest = new UpdateMediatorKeysRequest(mediationResult.Value.MediatorEndpoint, mediationResult.Value.MediatorDid, localDid.Value.PeerDid.Value, new List<string>() { someTestKeysToAdd.Value.PeerDid.Value }, new List<string>());
        var addMediatorKeysHandler = new UpdateMediatorKeysHandler(_mediatorMock.Object, _httpClient, simpleDidDocResolver, secretResolverInMemory);
        var addKeyResult = await addMediatorKeysHandler.Handle(addKeyRequest, CancellationToken.None);

        // Assert
        addKeyResult.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task AddKeyAndThenRemoveKeyToExistingConnection()
    {
        // Arrange
        var oobInvitationRootsLocal =
            "eyJ0eXBlIjoiaHR0cHM6Ly9kaWRjb21tLm9yZy9vdXQtb2YtYmFuZC8yLjAvaW52aXRhdGlvbiIsImlkIjoiNGZjN2Q3NDYtMzk2Ny00NjFjLTg5MTAtMWM5YTBmMjdkYjQ0IiwiZnJvbSI6ImRpZDpwZWVyOjIuRXo2TFNkRWNzVnZ6ZTNjWkpxaTFLRFdyU2N5MmFINW9IOFlkUVJRaTZmNVpIN1lGMi5WejZNa284aXllUDRITTFpS3ZuY2o3TkVRQ3JjeXpQN1Y1VW1ad2N1QXN6NWhZRkJlLlNleUpwWkNJNkltNWxkeTFwWkNJc0luUWlPaUprYlNJc0luTWlPaUpvZEhSd09pOHZNVEkzTGpBdU1DNHhPamd3TURBaUxDSmhJanBiSW1ScFpHTnZiVzB2ZGpJaVhYMCIsImJvZHkiOnsiZ29hbF9jb2RlIjoicmVxdWVzdC1tZWRpYXRlIiwiZ29hbCI6IlJlcXVlc3RNZWRpYXRlIiwibGFiZWwiOiJNZWRpYXRvciIsImFjY2VwdCI6WyJkaWRjb21tL3YyIl19fQ";

        var secretResolverInMemory = new SecretResolverInMemory();
        var simpleDidDocResolver = new SimpleDidDocResolver();
        _createPeerDidHandler = new CreatePeerDidHandler(secretResolverInMemory);

        var localDid = await _createPeerDidHandler.Handle(new CreatePeerDidRequest(), cancellationToken: new CancellationToken());
        var request = new RequestMediationRequest(oobInvitationRootsLocal, localDid.Value.PeerDid.Value);

        _requestMediationHandler = new RequestMediationHandler(_mediatorMock.Object, _httpClient, simpleDidDocResolver, secretResolverInMemory);
        var mediationResult = await _requestMediationHandler.Handle(request, CancellationToken.None);

        mediationResult.IsSuccess.Should().BeTrue();
        mediationResult.Value.MediationGranted.Should().BeTrue();

        var someTestKeysToAdd = await _createPeerDidHandler.Handle(new CreatePeerDidRequest(), cancellationToken: new CancellationToken());
        var addKeyRequest = new UpdateMediatorKeysRequest(mediationResult.Value.MediatorEndpoint, mediationResult.Value.MediatorDid, localDid.Value.PeerDid.Value, new List<string>() { someTestKeysToAdd.Value.PeerDid.Value }, new List<string>());
        var updateMediatorKeysHandler = new UpdateMediatorKeysHandler(_mediatorMock.Object, _httpClient, simpleDidDocResolver, secretResolverInMemory);
        var addKeyResult = await updateMediatorKeysHandler.Handle(addKeyRequest, CancellationToken.None);

        addKeyResult.IsSuccess.Should().BeTrue();

        // Act
        var removeKeyRequest = new UpdateMediatorKeysRequest(mediationResult.Value.MediatorEndpoint, mediationResult.Value.MediatorDid, localDid.Value.PeerDid.Value, new List<string>(), new List<string>() { someTestKeysToAdd.Value.PeerDid.Value });
        var removeKeyResult = await updateMediatorKeysHandler.Handle(removeKeyRequest, CancellationToken.None);


        // Assert
        removeKeyResult.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task AddKeyToExistingConnectionAndQuery()
    {
        // Arrange
        var oobInvitationRootsLocal =
            "eyJ0eXBlIjoiaHR0cHM6Ly9kaWRjb21tLm9yZy9vdXQtb2YtYmFuZC8yLjAvaW52aXRhdGlvbiIsImlkIjoiNGZjN2Q3NDYtMzk2Ny00NjFjLTg5MTAtMWM5YTBmMjdkYjQ0IiwiZnJvbSI6ImRpZDpwZWVyOjIuRXo2TFNkRWNzVnZ6ZTNjWkpxaTFLRFdyU2N5MmFINW9IOFlkUVJRaTZmNVpIN1lGMi5WejZNa284aXllUDRITTFpS3ZuY2o3TkVRQ3JjeXpQN1Y1VW1ad2N1QXN6NWhZRkJlLlNleUpwWkNJNkltNWxkeTFwWkNJc0luUWlPaUprYlNJc0luTWlPaUpvZEhSd09pOHZNVEkzTGpBdU1DNHhPamd3TURBaUxDSmhJanBiSW1ScFpHTnZiVzB2ZGpJaVhYMCIsImJvZHkiOnsiZ29hbF9jb2RlIjoicmVxdWVzdC1tZWRpYXRlIiwiZ29hbCI6IlJlcXVlc3RNZWRpYXRlIiwibGFiZWwiOiJNZWRpYXRvciIsImFjY2VwdCI6WyJkaWRjb21tL3YyIl19fQ";

        var secretResolverInMemory = new SecretResolverInMemory();
        var simpleDidDocResolver = new SimpleDidDocResolver();
        _createPeerDidHandler = new CreatePeerDidHandler(secretResolverInMemory);

        var localDid = await _createPeerDidHandler.Handle(new CreatePeerDidRequest(), cancellationToken: new CancellationToken());
        var request = new RequestMediationRequest(oobInvitationRootsLocal, localDid.Value.PeerDid.Value);

        _requestMediationHandler = new RequestMediationHandler(_mediatorMock.Object, _httpClient, simpleDidDocResolver, secretResolverInMemory);
        var mediationResult = await _requestMediationHandler.Handle(request, CancellationToken.None);

        mediationResult.IsSuccess.Should().BeTrue();
        mediationResult.Value.MediationGranted.Should().BeTrue();

        var someTestKeysToAdd = await _createPeerDidHandler.Handle(new CreatePeerDidRequest(), cancellationToken: new CancellationToken());
        var addKeyRequest = new UpdateMediatorKeysRequest(mediationResult.Value.MediatorEndpoint, mediationResult.Value.MediatorDid, localDid.Value.PeerDid.Value, new List<string>() { someTestKeysToAdd.Value.PeerDid.Value }, new List<string>());
        var addMediatorKeysHandler = new UpdateMediatorKeysHandler(_mediatorMock.Object, _httpClient, simpleDidDocResolver, secretResolverInMemory);
        var addKeyResult = await addMediatorKeysHandler.Handle(addKeyRequest, CancellationToken.None);

        addKeyResult.IsSuccess.Should().BeTrue();

        // Act
        var queryKeysRequest = new QueryMediatorKeysRequest(mediationResult.Value.MediatorEndpoint, mediationResult.Value.MediatorDid, localDid.Value.PeerDid.Value);
        var queryMediatorKeysHandler = new QueryMediatorKeysHandler(_mediatorMock.Object, _httpClient, simpleDidDocResolver, secretResolverInMemory);
        var queryKeyResult = await queryMediatorKeysHandler.Handle(queryKeysRequest, CancellationToken.None);
        
        // Assert
        queryKeyResult.IsSuccess.Should().BeTrue();
        queryKeyResult.Value.Count.Should().Be(1);
        queryKeyResult.Value[0].Should().Be(someTestKeysToAdd.Value.PeerDid.Value);
    }
}