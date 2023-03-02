namespace Blocktrust.Mediator.Client.IntegrationsTests;

using Commands.MediatorCoordinator.InquireMediation;
using Common;
using Common.Commands.CreatePeerDid;
using DIDComm.Secrets;
using FluentAssertions;
using MediatR;
using Moq;
using Xunit;

public class InquireMediationTests
{
    private readonly Mock<IMediator> _mediatorMock;
    private InquireMediationHandler _inquireMediationHandler;
    private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
    private readonly CreatePeerDidHandler _createPeerDidHandler;

    public InquireMediationTests()
    {
        _mediatorMock = new Mock<IMediator>();
        // _createPeerDidHandler = new CreatePeerDidHandler();
        _httpMessageHandlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);

        // _mediatorMock.Setup(p => p.Send(It.IsAny<CreatePeerDidRequest>(), It.IsAny<CancellationToken>()))
        //     .Returns(async (CreatePeerDidRequest request, CancellationToken token) => await _createPeerDidHandler.Handle(request, token));
    }

    /// <summary>
    /// This tests are testing the client and master projects in integration
    /// </summary>
    
    [Fact]
    public async Task MediatonFlowWithoutDidcomm()
    {
        // // The client creates a request for mediation
        // var oobInvitationRootsLocal =
        //     "eyJ0eXBlIjoiaHR0cHM6Ly9kaWRjb21tLm9yZy9vdXQtb2YtYmFuZC8yLjAvaW52aXRhdGlvbiIsImlkIjoiMmI5ZjFiZDMtMGQxZC00ODAzLThkZTctNTBhMjM5OTZkOGM2IiwiZnJvbSI6ImRpZDpwZWVyOjIuRXo2TFNyNTU3a25wdHJmcUVQNnFGeXd3N2hnQlU3aDhwV1NQVnFrQjUzV2h5ZXV2di5WejZNa3dBSmNlZUJBMnVWRU1LTGoycFZUdUpVeDQzelpnQlF1b2hkV1k5WThiNDh6LlNleUpwWkNJNkltNWxkeTFwWkNJc0luUWlPaUprYlNJc0luTWlPaUpvZEhSd09pOHZNVEkzTGpBdU1DNHhPamd3TURBaUxDSmhJanBiSW1ScFpHTnZiVzB2ZGpJaVhYMCIsImJvZHkiOnsiZ29hbF9jb2RlIjoicmVxdWVzdC1tZWRpYXRlIiwiZ29hbCI6IlJlcXVlc3RNZWRpYXRlIiwibGFiZWwiOiJNZWRpYXRvciIsImFjY2VwdCI6WyJkaWRjb21tL3YyIl19fQ";
        // var request = new InquireMediationRequest(oobInvitationRootsLocal); 
        //
        //
        //
        //
        
        
        // _inquireMediationHandler = new InquireMediationHandler(_mediatorMock.Object, _httpClient, new SimpleDidDocResolver(), new SecretResolverInMemory());
        // We don't mock the complete HTTP request flow but assume that the mediator is identitified this request
        // as a correct mediate-request and calls the AnswerMedidationHandler
        
        
        
        
        
        
        // Arrange
        // var oobInvitationRootsLocal =
        //     "eyJ0eXBlIjoiaHR0cHM6Ly9kaWRjb21tLm9yZy9vdXQtb2YtYmFuZC8yLjAvaW52aXRhdGlvbiIsImlkIjoiMmI5ZjFiZDMtMGQxZC00ODAzLThkZTctNTBhMjM5OTZkOGM2IiwiZnJvbSI6ImRpZDpwZWVyOjIuRXo2TFNyNTU3a25wdHJmcUVQNnFGeXd3N2hnQlU3aDhwV1NQVnFrQjUzV2h5ZXV2di5WejZNa3dBSmNlZUJBMnVWRU1LTGoycFZUdUpVeDQzelpnQlF1b2hkV1k5WThiNDh6LlNleUpwWkNJNkltNWxkeTFwWkNJc0luUWlPaUprYlNJc0luTWlPaUpvZEhSd09pOHZNVEkzTGpBdU1DNHhPamd3TURBaUxDSmhJanBiSW1ScFpHTnZiVzB2ZGpJaVhYMCIsImJvZHkiOnsiZ29hbF9jb2RlIjoicmVxdWVzdC1tZWRpYXRlIiwiZ29hbCI6IlJlcXVlc3RNZWRpYXRlIiwibGFiZWwiOiJNZWRpYXRvciIsImFjY2VwdCI6WyJkaWRjb21tL3YyIl19fQ";
        // var request = new InquireMediationRequest(oobInvitationRootsLocal);

        // Act
        // _inquireMediationHandler = new InquireMediationHandler(_mediatorMock.Object, _httpClient, new SimpleDidDocResolver(), new SecretResolverInMemory());
        // var result = await _inquireMediationHandler.Handle(request, CancellationToken.None);

        // Assert
        // result.IsSuccess.Should().BeTrue();
        // result.Value.MediationGranted.Should().BeTrue();
        // result.Value.RoutingDid.Should().NotBeNullOrEmpty();
    }
}