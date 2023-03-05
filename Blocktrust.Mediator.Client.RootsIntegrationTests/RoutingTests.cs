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

public class RoutingTests
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly HttpClient _httpClient;
    private RequestMediationHandler _requestMediationHandler;
    private CreatePeerDidHandler _createPeerDidHandler;

    public RoutingTests()
    {
        _mediatorMock = new Mock<IMediator>();
        _httpClient = new HttpClient();
    }

    /// <summary>
    /// This tests assumes that the Roots Mediator is running on http://127.0.0.1:22734
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



 


}