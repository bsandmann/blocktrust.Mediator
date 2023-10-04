namespace Blocktrust.Mediator.Client.PrismIntegrationTests;

using Blocktrust.DIDComm.Secrets;
using Blocktrust.Mediator.Client.Commands.MediatorCoordinator.QueryKeys;
using Blocktrust.Mediator.Client.Commands.MediatorCoordinator.RequestMediation;
using Blocktrust.Mediator.Client.Commands.MediatorCoordinator.UpdateKeys;
using Blocktrust.Mediator.Common;
using Blocktrust.Mediator.Common.Commands.CreatePeerDid;
using FluentAssertions;
using Xunit;

public class MediationCoordinatorTests
{
    private readonly HttpClient _httpClient;
    private RequestMediationHandler _requestMediationHandler;
    private CreatePeerDidHandler _createPeerDidHandler;

    public MediationCoordinatorTests()
    {
        _httpClient = new HttpClient();
    }

    /// <summary>
    /// This tests assumes that the PRISM Mediator is running on https://beta-mediator.atalaprism.io
    /// </summary>
    [Fact]
    public async Task InitiateMediateRequestsGetsGranted()
    {
        // Arrange
        var oobInvitationPrismHosted =
            "eyJpZCI6ImY4ODQ5OTY4LTQ2MWQtNDRlYS1iYzMwLWQ0MTc5MzQ3YzZjYSIsInR5cGUiOiJodHRwczovL2RpZGNvbW0ub3JnL291dC1vZi1iYW5kLzIuMC9pbnZpdGF0aW9uIiwiZnJvbSI6ImRpZDpwZWVyOjIuRXo2TFNnaHdTRTQzN3duREUxcHQzWDZoVkRVUXpTanNIemlucFgzWEZ2TWpSQW03eS5WejZNa2hoMWU1Q0VZWXE2SkJVY1RaNkNwMnJhbkNXUnJ2N1lheDNMZTRONTlSNmRkLlNleUowSWpvaVpHMGlMQ0p6SWpvaWFIUjBjSE02THk5aVpYUmhMVzFsWkdsaGRHOXlMbUYwWVd4aGNISnBjMjB1YVc4aUxDSnlJanBiWFN3aVlTSTZXeUprYVdSamIyMXRMM1l5SWwxOSIsImJvZHkiOnsiZ29hbF9jb2RlIjoicmVxdWVzdC1tZWRpYXRlIiwiZ29hbCI6IlJlcXVlc3RNZWRpYXRlIiwiYWNjZXB0IjpbImRpZGNvbW0vdjIiXX0sInR5cCI6ImFwcGxpY2F0aW9uL2RpZGNvbW0tcGxhaW4ranNvbiJ9";

        var secretResolverInMemory = new SecretResolverInMemory();
        _createPeerDidHandler = new CreatePeerDidHandler(secretResolverInMemory);

        var localDid = await _createPeerDidHandler.Handle(new CreatePeerDidRequest(), cancellationToken: new CancellationToken());
        var request = new RequestMediationRequest(oobInvitationPrismHosted , localDid.Value.PeerDid.Value);

        // Act
        _requestMediationHandler = new RequestMediationHandler(_httpClient, new SimpleDidDocResolver(), secretResolverInMemory);
        var result = await _requestMediationHandler.Handle(request, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.MediationGranted.Should().BeTrue();
        result.Value.RoutingDid.Should().NotBeNullOrEmpty();
    }


    /// <summary>
    /// This tests assumes that the PRISM Mediator is running on https://beta-mediator.atalaprism.io
    /// </summary>
    [Fact]
    public async Task InitiateMediateRequestsGetsDeniedTheSecondTimeBecauseOfExistingConnection()
    {
        // Arrange
        var oobInvitationRootsLocal =
            "eyJ0eXBlIjoiaHR0cHM6Ly9kaWRjb21tLm9yZy9vdXQtb2YtYmFuZC8yLjAvaW52aXRhdGlvbiIsImlkIjoiNGZlYjY4NjctMGRkYS00MWRkLWJiYjUtNWU5MDJjZDZjNzdkIiwiZnJvbSI6ImRpZDpwZWVyOjIuRXo2TFNxSmZnYkU5QTJBbUI2UkpmcEpDZ1J6b2pLUmdNRHNKM0hhMXlWandxcG1TQi5WejZNa29meVZiYWJDSnNGb1BxMjZNUmU3bURaQ2hxaE0xRzJEamtNek5KUEVRYVgxLlNleUpwWkNJNkltNWxkeTFwWkNJc0luUWlPaUprYlNJc0luTWlPaUpvZEhSd09pOHZNVEkzTGpBdU1DNHhPamd3TURBaUxDSmhJanBiSW1ScFpHTnZiVzB2ZGpJaVhYMCIsImJvZHkiOnsiZ29hbF9jb2RlIjoicmVxdWVzdC1tZWRpYXRlIiwiZ29hbCI6IlJlcXVlc3RNZWRpYXRlIiwibGFiZWwiOiJNZWRpYXRvciIsImFjY2VwdCI6WyJkaWRjb21tL3YyIl19fQ";

        var secretResolverInMemory = new SecretResolverInMemory();
        _createPeerDidHandler = new CreatePeerDidHandler(secretResolverInMemory);

        var localDid = await _createPeerDidHandler.Handle(new CreatePeerDidRequest(), cancellationToken: new CancellationToken());
        var firstRequest = new RequestMediationRequest(oobInvitationRootsLocal, localDid.Value.PeerDid.Value);

        _requestMediationHandler = new RequestMediationHandler(_httpClient, new SimpleDidDocResolver(), secretResolverInMemory);
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

    /// <summary>
    /// This tests assumes that the PRISM Mediator is running on https://beta-mediator.atalaprism.io
    /// </summary>
    [Fact]
    public async Task AddKeyToExistingConnection()
    {
        // Arrange
        var oobInvitationRootsLocal =
            "eyJ0eXBlIjoiaHR0cHM6Ly9kaWRjb21tLm9yZy9vdXQtb2YtYmFuZC8yLjAvaW52aXRhdGlvbiIsImlkIjoiNGZlYjY4NjctMGRkYS00MWRkLWJiYjUtNWU5MDJjZDZjNzdkIiwiZnJvbSI6ImRpZDpwZWVyOjIuRXo2TFNxSmZnYkU5QTJBbUI2UkpmcEpDZ1J6b2pLUmdNRHNKM0hhMXlWandxcG1TQi5WejZNa29meVZiYWJDSnNGb1BxMjZNUmU3bURaQ2hxaE0xRzJEamtNek5KUEVRYVgxLlNleUpwWkNJNkltNWxkeTFwWkNJc0luUWlPaUprYlNJc0luTWlPaUpvZEhSd09pOHZNVEkzTGpBdU1DNHhPamd3TURBaUxDSmhJanBiSW1ScFpHTnZiVzB2ZGpJaVhYMCIsImJvZHkiOnsiZ29hbF9jb2RlIjoicmVxdWVzdC1tZWRpYXRlIiwiZ29hbCI6IlJlcXVlc3RNZWRpYXRlIiwibGFiZWwiOiJNZWRpYXRvciIsImFjY2VwdCI6WyJkaWRjb21tL3YyIl19fQ";

        var secretResolverInMemory = new SecretResolverInMemory();
        var simpleDidDocResolver = new SimpleDidDocResolver();
        _createPeerDidHandler = new CreatePeerDidHandler(secretResolverInMemory);

        var localDid = await _createPeerDidHandler.Handle(new CreatePeerDidRequest(), cancellationToken: new CancellationToken());
        var request = new RequestMediationRequest(oobInvitationRootsLocal, localDid.Value.PeerDid.Value);

        _requestMediationHandler = new RequestMediationHandler(_httpClient, simpleDidDocResolver, secretResolverInMemory);
        var mediationResult = await _requestMediationHandler.Handle(request, CancellationToken.None);

        mediationResult.IsSuccess.Should().BeTrue();
        mediationResult.Value.MediationGranted.Should().BeTrue();

        // Act
        var someTestKeysToAdd = await _createPeerDidHandler.Handle(new CreatePeerDidRequest(), cancellationToken: new CancellationToken());
        var addKeyRequest = new UpdateMediatorKeysRequest(mediationResult.Value.MediatorEndpoint, mediationResult.Value.MediatorDid, localDid.Value.PeerDid.Value, new List<string>() { someTestKeysToAdd.Value.PeerDid.Value }, new List<string>());
        var addMediatorKeysHandler = new UpdateMediatorKeysHandler(_httpClient, simpleDidDocResolver, secretResolverInMemory);
        var addKeyResult = await addMediatorKeysHandler.Handle(addKeyRequest, CancellationToken.None);

        // Assert
        addKeyResult.IsSuccess.Should().BeTrue();
    }

    /// <summary>
    /// This tests assumes that the PRISM Mediator is running on https://beta-mediator.atalaprism.io
    /// </summary>
    [Fact]
    public async Task AddKeyAndThenRemoveKeyToExistingConnection()
    {
        // Arrange
        var oobInvitationRootsLocal =
            "eyJ0eXBlIjoiaHR0cHM6Ly9kaWRjb21tLm9yZy9vdXQtb2YtYmFuZC8yLjAvaW52aXRhdGlvbiIsImlkIjoiNGZlYjY4NjctMGRkYS00MWRkLWJiYjUtNWU5MDJjZDZjNzdkIiwiZnJvbSI6ImRpZDpwZWVyOjIuRXo2TFNxSmZnYkU5QTJBbUI2UkpmcEpDZ1J6b2pLUmdNRHNKM0hhMXlWandxcG1TQi5WejZNa29meVZiYWJDSnNGb1BxMjZNUmU3bURaQ2hxaE0xRzJEamtNek5KUEVRYVgxLlNleUpwWkNJNkltNWxkeTFwWkNJc0luUWlPaUprYlNJc0luTWlPaUpvZEhSd09pOHZNVEkzTGpBdU1DNHhPamd3TURBaUxDSmhJanBiSW1ScFpHTnZiVzB2ZGpJaVhYMCIsImJvZHkiOnsiZ29hbF9jb2RlIjoicmVxdWVzdC1tZWRpYXRlIiwiZ29hbCI6IlJlcXVlc3RNZWRpYXRlIiwibGFiZWwiOiJNZWRpYXRvciIsImFjY2VwdCI6WyJkaWRjb21tL3YyIl19fQ";

        var secretResolverInMemory = new SecretResolverInMemory();
        var simpleDidDocResolver = new SimpleDidDocResolver();
        _createPeerDidHandler = new CreatePeerDidHandler(secretResolverInMemory);

        var localDid = await _createPeerDidHandler.Handle(new CreatePeerDidRequest(), cancellationToken: new CancellationToken());
        var request = new RequestMediationRequest(oobInvitationRootsLocal, localDid.Value.PeerDid.Value);

        _requestMediationHandler = new RequestMediationHandler(_httpClient, simpleDidDocResolver, secretResolverInMemory);
        var mediationResult = await _requestMediationHandler.Handle(request, CancellationToken.None);

        mediationResult.IsSuccess.Should().BeTrue();
        mediationResult.Value.MediationGranted.Should().BeTrue();

        var someTestKeysToAdd = await _createPeerDidHandler.Handle(new CreatePeerDidRequest(), cancellationToken: new CancellationToken());
        var addKeyRequest = new UpdateMediatorKeysRequest(mediationResult.Value.MediatorEndpoint, mediationResult.Value.MediatorDid, localDid.Value.PeerDid.Value, new List<string>() { someTestKeysToAdd.Value.PeerDid.Value }, new List<string>());
        var updateMediatorKeysHandler = new UpdateMediatorKeysHandler(_httpClient, simpleDidDocResolver, secretResolverInMemory);
        var addKeyResult = await updateMediatorKeysHandler.Handle(addKeyRequest, CancellationToken.None);

        addKeyResult.IsSuccess.Should().BeTrue();

        // Act
        var removeKeyRequest = new UpdateMediatorKeysRequest(mediationResult.Value.MediatorEndpoint, mediationResult.Value.MediatorDid, localDid.Value.PeerDid.Value, new List<string>(), new List<string>() { someTestKeysToAdd.Value.PeerDid.Value });
        var removeKeyResult = await updateMediatorKeysHandler.Handle(removeKeyRequest, CancellationToken.None);


        // Assert
        removeKeyResult.IsSuccess.Should().BeTrue();
    }

    /// <summary>
    /// This tests assumes that the PRISM Mediator is running on https://beta-mediator.atalaprism.io
    /// </summary>
    [Fact]
    public async Task AddKeyToExistingConnectionAndQuery()
    {
        // Arrange
        var oobInvitationRootsLocal =
            "eyJ0eXBlIjoiaHR0cHM6Ly9kaWRjb21tLm9yZy9vdXQtb2YtYmFuZC8yLjAvaW52aXRhdGlvbiIsImlkIjoiNGZlYjY4NjctMGRkYS00MWRkLWJiYjUtNWU5MDJjZDZjNzdkIiwiZnJvbSI6ImRpZDpwZWVyOjIuRXo2TFNxSmZnYkU5QTJBbUI2UkpmcEpDZ1J6b2pLUmdNRHNKM0hhMXlWandxcG1TQi5WejZNa29meVZiYWJDSnNGb1BxMjZNUmU3bURaQ2hxaE0xRzJEamtNek5KUEVRYVgxLlNleUpwWkNJNkltNWxkeTFwWkNJc0luUWlPaUprYlNJc0luTWlPaUpvZEhSd09pOHZNVEkzTGpBdU1DNHhPamd3TURBaUxDSmhJanBiSW1ScFpHTnZiVzB2ZGpJaVhYMCIsImJvZHkiOnsiZ29hbF9jb2RlIjoicmVxdWVzdC1tZWRpYXRlIiwiZ29hbCI6IlJlcXVlc3RNZWRpYXRlIiwibGFiZWwiOiJNZWRpYXRvciIsImFjY2VwdCI6WyJkaWRjb21tL3YyIl19fQ";

        var secretResolverInMemory = new SecretResolverInMemory();
        var simpleDidDocResolver = new SimpleDidDocResolver();
        _createPeerDidHandler = new CreatePeerDidHandler(secretResolverInMemory);

        var localDid = await _createPeerDidHandler.Handle(new CreatePeerDidRequest(), cancellationToken: new CancellationToken());
        var request = new RequestMediationRequest(oobInvitationRootsLocal, localDid.Value.PeerDid.Value);

        _requestMediationHandler = new RequestMediationHandler(_httpClient, simpleDidDocResolver, secretResolverInMemory);
        var mediationResult = await _requestMediationHandler.Handle(request, CancellationToken.None);

        mediationResult.IsSuccess.Should().BeTrue();
        mediationResult.Value.MediationGranted.Should().BeTrue();

        var someTestKeysToAdd = await _createPeerDidHandler.Handle(new CreatePeerDidRequest(), cancellationToken: new CancellationToken());
        var addKeyRequest = new UpdateMediatorKeysRequest(mediationResult.Value.MediatorEndpoint, mediationResult.Value.MediatorDid, localDid.Value.PeerDid.Value, new List<string>() { someTestKeysToAdd.Value.PeerDid.Value }, new List<string>());
        var addMediatorKeysHandler = new UpdateMediatorKeysHandler(_httpClient, simpleDidDocResolver, secretResolverInMemory);
        var addKeyResult = await addMediatorKeysHandler.Handle(addKeyRequest, CancellationToken.None);

        addKeyResult.IsSuccess.Should().BeTrue();

        // Act
        var queryKeysRequest = new QueryMediatorKeysRequest(mediationResult.Value.MediatorEndpoint, mediationResult.Value.MediatorDid, localDid.Value.PeerDid.Value);
        var queryMediatorKeysHandler = new QueryMediatorKeysHandler(_httpClient, simpleDidDocResolver, secretResolverInMemory);
        var queryKeyResult = await queryMediatorKeysHandler.Handle(queryKeysRequest, CancellationToken.None);

        // Assert
        queryKeyResult.IsSuccess.Should().BeTrue();
        queryKeyResult.Value.RegisteredMediatorKeys.Should().NotBeNull();
        queryKeyResult.Value.RegisteredMediatorKeys.Count.Should().Be(1);
        queryKeyResult.Value.RegisteredMediatorKeys[0].Should().Be(someTestKeysToAdd.Value.PeerDid.Value);
    }
}