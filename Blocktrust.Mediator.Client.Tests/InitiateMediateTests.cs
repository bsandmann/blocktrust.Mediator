using Blocktrust.Mediator.Client.Commands.InitiateMediate;
using MediatR;
using Moq;
using Xunit;

namespace Blocktrust.Mediator.Client.Tests;

using Common.Commands.CreatePeerDid;

public class InitiateMediateTests
{
    private readonly Mock<IMediator> _mediatorMock;
    // private readonly Mock<HttpClient> _httpClientMock;
    private readonly HttpClient _httpClient;
    private readonly InitiateMediateHandler _initiateMediateHandler;
    private readonly CreatePeerDidHandler _createPeerDidHandler;

    public InitiateMediateTests()
    {
        _mediatorMock = new Mock<IMediator>();
        // _httpClientMock = new Mock<HttpClient>();
        _httpClient = new HttpClient();
        _initiateMediateHandler = new InitiateMediateHandler(_mediatorMock.Object, _httpClient);
        _createPeerDidHandler = new CreatePeerDidHandler();

        _mediatorMock.Setup(p => p.Send(It.IsAny<CreatePeerDidRequest>(), It.IsAny<CancellationToken>()))
            .Returns(async (CreatePeerDidRequest request, CancellationToken token) => await _createPeerDidHandler.Handle(request, token));
    }

    [Fact]
    public async Task InitiateMediateRequestsGetsCreated()
    {
        // Arrange
        var oobInvitationRootsLocal =
            "eyJ0eXBlIjoiaHR0cHM6Ly9kaWRjb21tLm9yZy9vdXQtb2YtYmFuZC8yLjAvaW52aXRhdGlvbiIsImlkIjoiMmI5ZjFiZDMtMGQxZC00ODAzLThkZTctNTBhMjM5OTZkOGM2IiwiZnJvbSI6ImRpZDpwZWVyOjIuRXo2TFNyNTU3a25wdHJmcUVQNnFGeXd3N2hnQlU3aDhwV1NQVnFrQjUzV2h5ZXV2di5WejZNa3dBSmNlZUJBMnVWRU1LTGoycFZUdUpVeDQzelpnQlF1b2hkV1k5WThiNDh6LlNleUpwWkNJNkltNWxkeTFwWkNJc0luUWlPaUprYlNJc0luTWlPaUpvZEhSd09pOHZNVEkzTGpBdU1DNHhPamd3TURBaUxDSmhJanBiSW1ScFpHTnZiVzB2ZGpJaVhYMCIsImJvZHkiOnsiZ29hbF9jb2RlIjoicmVxdWVzdC1tZWRpYXRlIiwiZ29hbCI6IlJlcXVlc3RNZWRpYXRlIiwibGFiZWwiOiJNZWRpYXRvciIsImFjY2VwdCI6WyJkaWRjb21tL3YyIl19fQ";
        // "eyJ0eXBlIjoiaHR0cHM6Ly9kaWRjb21tLm9yZy9vdXQtb2YtYmFuZC8yLjAvaW52aXRhdGlvbiIsImlkIjoiNzJkNmM4YjMtNmJhNy00MzA0LTkzZDMtYTY5YTA0ZTRmMjJiIiwiZnJvbSI6ImRpZDpwZWVyOjIuRXo2TFNtczU1NVloRnRobjFXVjhjaURCcFptODZoSzl0cDgzV29qSlVteFBHazFoWi5WejZNa21kQmpNeUI0VFM1VWJiUXc1NHN6bTh5dk1NZjFmdEdWMnNRVllBeGFlV2hFLlNleUpwWkNJNkltNWxkeTFwWkNJc0luUWlPaUprYlNJc0luTWlPaUpvZEhSd2N6b3ZMMjFsWkdsaGRHOXlMbkp2YjNSemFXUXVZMnh2ZFdRaUxDSmhJanBiSW1ScFpHTnZiVzB2ZGpJaVhYMCIsImJvZHkiOnsiZ29hbF9jb2RlIjoicmVxdWVzdC1tZWRpYXRlIiwiZ29hbCI6IlJlcXVlc3RNZWRpYXRlIiwibGFiZWwiOiJNZWRpYXRvciIsImFjY2VwdCI6WyJkaWRjb21tL3YyIl19fQ";
        var oobInvitationPrismAgent =
            "eyJpZCI6Ijc1MDQyODAxLWQ5NGQtNDExMy1hMGFlLTU5ODYxMjUxMDU1NyIsInR5cGUiOiJodHRwczovL2RpZGNvbW0ub3JnL291dC1vZi1iYW5kLzIuMC9pbnZpdGF0aW9uIiwiZnJvbSI6ImRpZDpwZWVyOjIuRXo2TFN0RExIOHp2ZkJOdnBpWnV2OXMzV2l6Y0pXSGhhaWtKZnl2d0I1RkdjUEdDaS5WejZNa2lFemhCNVVueTFCdEhQVm1IWEtXaTVHUUx5RjNpaEVjenV2VGlaVlNTdkpmLlNleUowSWpvaVpHMGlMQ0p6SWpvaWFIUjBjSE02THk5aVp6WTFhaTVoZEdGc1lYQnlhWE50TG1sdkwzQnlhWE50TFdGblpXNTBMMlJwWkdOdmJXMGlMQ0p5SWpwYlhTd2lZU0k2V3lKa2FXUmpiMjF0TDNZeUlsMTkiLCJib2R5Ijp7ImdvYWxfY29kZSI6ImlvLmF0YWxhcHJpc20uY29ubmVjdCIsImdvYWwiOiJFc3RhYmxpc2ggYSB0cnVzdCBjb25uZWN0aW9uIGJldHdlZW4gdHdvIHBlZXJzIHVzaW5nIHRoZSBwcm90b2NvbCAnaHR0cHM6Ly9hdGFsYXByaXNtLmlvL21lcmN1cnkvY29ubmVjdGlvbnMvMS4wL3JlcXVlc3QnIiwiYWNjZXB0IjpbXX19";
        var request = new InitiateMediateRequest(oobInvitationPrismAgent);

        // Act
        var result = await _initiateMediateHandler.Handle(request, CancellationToken.None);

        // Assert
    }
}