namespace Blocktrust.Mediator.Client.LocalIntegrationTests;

using Commands.MediatorCoordinator.QueryKeys;
using Commands.MediatorCoordinator.RequestMediation;
using Commands.MediatorCoordinator.UpdateKeys;
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

public class MedidationCoordinatorTestsAgainstBlocktrustMediator
{
    private readonly Mock<IMediator> _mediatorMock;

    private CreatePeerDidHandler _createPeerDidHandler;
    private readonly GetSecretsHandler _getSecretsHandler;
    private readonly string _blocktrustMediatorUri = "https://localhost:7037/";
    private RequestMediationHandler _requestMediationHandler;
    private readonly HttpClient _httpClient;

    public MedidationCoordinatorTestsAgainstBlocktrustMediator()
    {
        _mediatorMock = new Mock<IMediator>();
        _httpClient = new HttpClient();
    }

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

        var localDid =  await _createPeerDidHandler.Handle(new CreatePeerDidRequest(),cancellationToken: new CancellationToken());
        var request = new RequestMediationRequest(oobInvitation, localDid.Value.PeerDid.Value);

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
        // First get the OOB from the running mediator
        var response = await _httpClient.GetAsync(_blocktrustMediatorUri + "oob_url");
        var resultContent = await response.Content.ReadAsStringAsync();
        var oob = resultContent.Split("=");
        var oobInvitation = oob[1]; 
        
        var secretResolverInMemory = new SecretResolverInMemory();
        _createPeerDidHandler = new CreatePeerDidHandler(secretResolverInMemory);

        var localDid = await _createPeerDidHandler.Handle(new CreatePeerDidRequest(), cancellationToken: new CancellationToken());
        var firstRequest = new RequestMediationRequest(oobInvitation, localDid.Value.PeerDid.Value);

        _requestMediationHandler = new RequestMediationHandler(_mediatorMock.Object, _httpClient, new SimpleDidDocResolver(), secretResolverInMemory);
        var firstResult = await _requestMediationHandler.Handle(firstRequest, CancellationToken.None);
        firstResult.IsSuccess.Should().BeTrue();
        firstResult.Value.MediationGranted.Should().BeTrue();
        firstResult.Value.RoutingDid.Should().NotBeNullOrEmpty();

        // Act
        var secondRequest = new RequestMediationRequest(oobInvitation, localDid.Value.PeerDid.Value);
        var secondResult = await _requestMediationHandler.Handle(secondRequest, CancellationToken.None);

        // Assert
        secondResult.IsSuccess.Should().BeTrue();
        secondResult.Value.MediationGranted.Should().BeFalse();
    }
    
     [Fact]
    public async Task AddKeyToExistingConnection()
    {
        // Arrange
        var response = await _httpClient.GetAsync(_blocktrustMediatorUri + "oob_url");
        var resultContent = await response.Content.ReadAsStringAsync();
        var oob = resultContent.Split("=");
        var oobInvitation = oob[1]; 
        
        
        var secretResolverInMemory = new SecretResolverInMemory();
        var simpleDidDocResolver = new SimpleDidDocResolver();
        _createPeerDidHandler = new CreatePeerDidHandler(secretResolverInMemory);

        var localDid = await _createPeerDidHandler.Handle(new CreatePeerDidRequest(), cancellationToken: new CancellationToken());
        var request = new RequestMediationRequest(oobInvitation, localDid.Value.PeerDid.Value);

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
        var response = await _httpClient.GetAsync(_blocktrustMediatorUri + "oob_url");
        var resultContent = await response.Content.ReadAsStringAsync();
        var oob = resultContent.Split("=");
        var oobInvitation = oob[1]; 
        
        var secretResolverInMemory = new SecretResolverInMemory();
        var simpleDidDocResolver = new SimpleDidDocResolver();
        _createPeerDidHandler = new CreatePeerDidHandler(secretResolverInMemory);

        var localDid = await _createPeerDidHandler.Handle(new CreatePeerDidRequest(), cancellationToken: new CancellationToken());
        var request = new RequestMediationRequest(oobInvitation, localDid.Value.PeerDid.Value);

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
        var response = await _httpClient.GetAsync(_blocktrustMediatorUri + "oob_url");
        var resultContent = await response.Content.ReadAsStringAsync();
        var oob = resultContent.Split("=");
        var oobInvitation = oob[1]; 
        
        var secretResolverInMemory = new SecretResolverInMemory();
        var simpleDidDocResolver = new SimpleDidDocResolver();
        _createPeerDidHandler = new CreatePeerDidHandler(secretResolverInMemory);

        var localDid = await _createPeerDidHandler.Handle(new CreatePeerDidRequest(), cancellationToken: new CancellationToken());
        var request = new RequestMediationRequest(oobInvitation, localDid.Value.PeerDid.Value);

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