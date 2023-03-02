namespace Blocktrust.Mediator.Client.RootsIntegrationTests;

using Blocktrust.Mediator.Common.Commands.CreatePeerDid;
using Commands.MediatorCoordinator.InquireMediation;
using Common;
using DIDComm.Secrets;
using FluentAssertions;
using MediatR;
using Moq;
using PeerDID.PeerDIDCreateResolve;
using Xunit;

public class InquireMediationTestsAgainstBlocktrustMediator
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly HttpClient _httpClient;
    private InquireMediationHandler _inquireMediationHandler;
    private readonly CreatePeerDidHandler _createPeerDidHandler;

    public InquireMediationTestsAgainstBlocktrustMediator()
    {
        _mediatorMock = new Mock<IMediator>();
        _httpClient = new HttpClient();
        _createPeerDidHandler = new CreatePeerDidHandler();

        _mediatorMock.Setup(p => p.Send(It.IsAny<CreatePeerDidRequest>(), It.IsAny<CancellationToken>()))
            .Returns(async (CreatePeerDidRequest request, CancellationToken token) => await _createPeerDidHandler.Handle(request, token));
    }

    /// <summary>
    /// This tests assumes that the Blocktrust Meditator is running on http:/localhost:8000
    /// </summary>
    
    [Fact]
    public async Task InitiateMediateRequestsGetsGranted()
    {
        // Arrange
        var oobInvitationRootsLocal =
            "eyJ0eXBlIjoiaHR0cHM6Ly9kaWRjb21tLm9yZy9vdXQtb2YtYmFuZC8yLjAvaW52aXRhdGlvbiIsImlkIjoiNGZjN2Q3NDYtMzk2Ny00NjFjLTg5MTAtMWM5YTBmMjdkYjQ0IiwiZnJvbSI6ImRpZDpwZWVyOjIuRXo2TFNkRWNzVnZ6ZTNjWkpxaTFLRFdyU2N5MmFINW9IOFlkUVJRaTZmNVpIN1lGMi5WejZNa284aXllUDRITTFpS3ZuY2o3TkVRQ3JjeXpQN1Y1VW1ad2N1QXN6NWhZRkJlLlNleUpwWkNJNkltNWxkeTFwWkNJc0luUWlPaUprYlNJc0luTWlPaUpvZEhSd09pOHZNVEkzTGpBdU1DNHhPamd3TURBaUxDSmhJanBiSW1ScFpHTnZiVzB2ZGpJaVhYMCIsImJvZHkiOnsiZ29hbF9jb2RlIjoicmVxdWVzdC1tZWRpYXRlIiwiZ29hbCI6IlJlcXVlc3RNZWRpYXRlIiwibGFiZWwiOiJNZWRpYXRvciIsImFjY2VwdCI6WyJkaWRjb21tL3YyIl19fQ";
        var request = new InquireMediationRequest(oobInvitationRootsLocal);

        // Act
        _inquireMediationHandler = new InquireMediationHandler(_mediatorMock.Object, _httpClient, new SimpleDidDocResolver(), new SecretResolverInMemory());
        var result = await _inquireMediationHandler.Handle(request, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.MediationGranted.Should().BeTrue();
        result.Value.RoutingDid.Should().NotBeNullOrEmpty();
    }
}