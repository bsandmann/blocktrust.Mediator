namespace Blocktrust.Mediator.Client.LocalIntegrationTests;

using Commands.MediatorCoordinator.InquireMediation;
using Common;
using Common.Commands.CreatePeerDid;
using Common.Models.OutOfBand;
using DIDComm.Secrets;
using FluentAssertions;
using FluentResults;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using Server;
using Server.Commands.Secrets.GetSecrets;

public class InquireMedidationTestsAgainstBlocktrustMediator
{
    private readonly Mock<IMediator> _mediatorMock;

    private CreatePeerDidHandler _createPeerDidHandler;
    private readonly GetSecretsHandler _getSecretsHandler;
    private readonly string _blocktrustMediatorUri = "https://localhost:7037/";
    private InquireMediationHandler _inquireMediationHandler;
    private readonly HttpClient _httpClient;

    public InquireMedidationTestsAgainstBlocktrustMediator()
    {
        _mediatorMock = new Mock<IMediator>();
        _httpClient = new HttpClient();
    }


    /// <summary>
    /// This tests assumes that the Blocktrust Meditator is running on https://localhost:7145/
    /// </summary>
    [Fact]
    public async Task InitiateMediateRequestsGetsGranted()
    {
        // First get the OOB from the running mediator
        var response = await _httpClient.GetAsync(_blocktrustMediatorUri + "oob_url");
        var resultContent = await response.Content.ReadAsStringAsync();
        var oob = resultContent.Split("=");
        var oobInvitation = oob[1];

        var secretResolverInMemory = new SecretResolverInMemory();
        _createPeerDidHandler = new CreatePeerDidHandler(secretResolverInMemory);

        // For the communication with the mediator we need a new peerDID
        var localDid =  await _createPeerDidHandler.Handle(new CreatePeerDidRequest(),cancellationToken: new CancellationToken());
        // Then send a request
        var request = new InquireMediationRequest(oobInvitation, localDid.Value.PeerDid.Value);

        // Act
        _inquireMediationHandler = new InquireMediationHandler(_mediatorMock.Object, _httpClient, new SimpleDidDocResolver(), secretResolverInMemory);
        var result = await _inquireMediationHandler.Handle(request, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.MediationGranted.Should().BeTrue();
        result.Value.RoutingDid.Should().NotBeNullOrEmpty();
    }
    
    /// <summary>
    /// This tests assumes that the Blocktrust Meditator is running on https://localhost:7145/
    /// </summary>
    [Fact]
    public async Task InitiateMediateRequestsGetsDeniedTheSecondTimeBecauseOfExistingConnection()
    {
       
    }
}