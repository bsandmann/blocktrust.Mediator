namespace Blocktrust.Mediator.Client.PrismMediatorIntegrationTests;

using Commands.MediatorCoordinator.QueryKeys;
using Commands.MediatorCoordinator.RequestMediation;
using Commands.MediatorCoordinator.UpdateKeys;
using Common;
using Common.Commands.CreatePeerDid;
using DIDComm.Secrets;
using FluentAssertions;

public class MediatorCoordinatorTests
{
    private CreatePeerDidHandler _createPeerDidHandler;
    private readonly string _prismMediatorUri = "https://beta-mediator.atalaprism.io/";
    private RequestMediationHandler _requestMediationHandler;
    private readonly HttpClient _httpClient;

    public MediatorCoordinatorTests()
    {
        _httpClient = new HttpClient();
    }

    /// <summary>
    /// This tests assumes that a PRISM Mediator is running on https://beta-mediator.atalaprism.io
    /// </summary>
    [Fact]
    public async Task InitiateMediateRequestsGetsGranted()
    {
        // First get the OOB from the running mediator
        var response = await _httpClient.GetAsync(_prismMediatorUri + "invitationOOB");
        var resultContent = await response.Content.ReadAsStringAsync();
        var oob = resultContent.Split("=");
        var oobInvitation = oob[1];

        var secretResolverInMemory = new SecretResolverInMemory();
        _createPeerDidHandler = new CreatePeerDidHandler(secretResolverInMemory);

        var localDid = await _createPeerDidHandler.Handle(new CreatePeerDidRequest(), cancellationToken: new CancellationToken());
        var request = new RequestMediationRequest(oobInvitation, localDid.Value.PeerDid.Value);

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
    /// THIS TEST FAILS AS OF October 04, 2023
    [Fact]
    public async Task InitiateMediateRequestsGetsDeniedTheSecondTimeBecauseOfExistingConnection()
    {
        // First get the OOB from the running mediator
        var response = await _httpClient.GetAsync(_prismMediatorUri + "invitationOOB");
        var resultContent = await response.Content.ReadAsStringAsync();
        var oob = resultContent.Split("=");
        var oobInvitation = oob[1];

        var secretResolverInMemory = new SecretResolverInMemory();
        _createPeerDidHandler = new CreatePeerDidHandler(secretResolverInMemory);

        var localDid = await _createPeerDidHandler.Handle(new CreatePeerDidRequest(), cancellationToken: new CancellationToken());
        var firstRequest = new RequestMediationRequest(oobInvitation, localDid.Value.PeerDid.Value);

        _requestMediationHandler = new RequestMediationHandler(_httpClient, new SimpleDidDocResolver(), secretResolverInMemory);
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

    /// <summary>
    /// This tests assumes that the PRISM Mediator is running on https://beta-mediator.atalaprism.io
    /// </summary>
    [Fact]
    public async Task AddKeyToExistingConnection()
    {
        // Arrange
        var response = await _httpClient.GetAsync(_prismMediatorUri + "invitationOOB");
        var resultContent = await response.Content.ReadAsStringAsync();
        var oob = resultContent.Split("=");
        var oobInvitation = oob[1];

        var secretResolverInMemory = new SecretResolverInMemory();
        var simpleDidDocResolver = new SimpleDidDocResolver();
        _createPeerDidHandler = new CreatePeerDidHandler(secretResolverInMemory);

        var localDid = await _createPeerDidHandler.Handle(new CreatePeerDidRequest(), cancellationToken: new CancellationToken());
        var request = new RequestMediationRequest(oobInvitation, localDid.Value.PeerDid.Value);

        _requestMediationHandler = new RequestMediationHandler(_httpClient, simpleDidDocResolver, secretResolverInMemory);
        var mediationResult = await _requestMediationHandler.Handle(request, CancellationToken.None);

        mediationResult.IsSuccess.Should().BeTrue();
        mediationResult.Value.MediationGranted.Should().BeTrue();
        mediationResult.Value.ProblemReport.Should().BeNull();

        // Act
        var someTestKeysToAdd = await _createPeerDidHandler.Handle(new CreatePeerDidRequest(), cancellationToken: new CancellationToken());
        var addKeyRequest = new UpdateMediatorKeysRequest(mediationResult.Value.MediatorEndpoint, mediationResult.Value.MediatorDid, localDid.Value.PeerDid.Value, new List<string>() { someTestKeysToAdd.Value.PeerDid.Value }, new List<string>());
        var addMediatorKeysHandler = new UpdateMediatorKeysHandler(_httpClient, simpleDidDocResolver, secretResolverInMemory);
        var addKeyResult = await addMediatorKeysHandler.Handle(addKeyRequest, CancellationToken.None);

        // Assert
        addKeyResult.IsSuccess.Should().BeTrue();
        addKeyResult.Value.ProblemReport.Should().BeNull();
    }

    /// <summary>
    /// This tests assumes that the PRISM Mediator is running on  https://beta-mediator.atalaprism.io
    /// </summary>
    [Fact]
    public async Task AddTwoKeysToExistingConnection()
    {
        // Arrange
        var response = await _httpClient.GetAsync(_prismMediatorUri + "invitationOOB");
        var resultContent = await response.Content.ReadAsStringAsync();
        var oob = resultContent.Split("=");
        var oobInvitation = oob[1];


        var secretResolverInMemory = new SecretResolverInMemory();
        var simpleDidDocResolver = new SimpleDidDocResolver();
        _createPeerDidHandler = new CreatePeerDidHandler(secretResolverInMemory);

        var localDid = await _createPeerDidHandler.Handle(new CreatePeerDidRequest(), cancellationToken: new CancellationToken());
        var request = new RequestMediationRequest(oobInvitation, localDid.Value.PeerDid.Value);

        _requestMediationHandler = new RequestMediationHandler(_httpClient, simpleDidDocResolver, secretResolverInMemory);
        var mediationResult = await _requestMediationHandler.Handle(request, CancellationToken.None);

        mediationResult.IsSuccess.Should().BeTrue();
        mediationResult.Value.MediationGranted.Should().BeTrue();
        mediationResult.Value.ProblemReport.Should().BeNull();

        // Act
        var someTestKeyToAdd = await _createPeerDidHandler.Handle(new CreatePeerDidRequest(), cancellationToken: new CancellationToken());
        var someOtherTestKeyToAdd = await _createPeerDidHandler.Handle(new CreatePeerDidRequest(), cancellationToken: new CancellationToken());
        var addKeyRequest = new UpdateMediatorKeysRequest(mediationResult.Value.MediatorEndpoint, mediationResult.Value.MediatorDid, localDid.Value.PeerDid.Value, new List<string>() { someTestKeyToAdd.Value.PeerDid.Value, someOtherTestKeyToAdd.Value.PeerDid.Value }, new List<string>());
        var addMediatorKeysHandler = new UpdateMediatorKeysHandler(_httpClient, simpleDidDocResolver, secretResolverInMemory);
        var addKeyResult = await addMediatorKeysHandler.Handle(addKeyRequest, CancellationToken.None);

        // Assert
        addKeyResult.IsSuccess.Should().BeTrue();
        addKeyResult.Value.ProblemReport.Should().BeNull();
    }

    /// <summary>
    /// This tests assumes that the PRISM Mediator is running on https://beta-mediator.atalaprism.io
    /// </summary>
    [Fact]
    public async Task AddKeyAndThenRemoveKeyToExistingConnection()
    {
        // Arrange
        var response = await _httpClient.GetAsync(_prismMediatorUri + "invitationOOB");
        var resultContent = await response.Content.ReadAsStringAsync();
        var oob = resultContent.Split("=");
        var oobInvitation = oob[1];

        var secretResolverInMemory = new SecretResolverInMemory();
        var simpleDidDocResolver = new SimpleDidDocResolver();
        _createPeerDidHandler = new CreatePeerDidHandler(secretResolverInMemory);

        var localDid = await _createPeerDidHandler.Handle(new CreatePeerDidRequest(), cancellationToken: new CancellationToken());
        var request = new RequestMediationRequest(oobInvitation, localDid.Value.PeerDid.Value);

        _requestMediationHandler = new RequestMediationHandler(_httpClient, simpleDidDocResolver, secretResolverInMemory);
        var mediationResult = await _requestMediationHandler.Handle(request, CancellationToken.None);

        mediationResult.IsSuccess.Should().BeTrue();
        mediationResult.Value.MediationGranted.Should().BeTrue();
        mediationResult.Value.ProblemReport.Should().BeNull();

        var someTestKeysToAdd = await _createPeerDidHandler.Handle(new CreatePeerDidRequest(), cancellationToken: new CancellationToken());
        var addKeyRequest = new UpdateMediatorKeysRequest(mediationResult.Value.MediatorEndpoint, mediationResult.Value.MediatorDid, localDid.Value.PeerDid.Value, new List<string>() { someTestKeysToAdd.Value.PeerDid.Value }, new List<string>());
        var updateMediatorKeysHandler = new UpdateMediatorKeysHandler(_httpClient, simpleDidDocResolver, secretResolverInMemory);
        var addKeyResult = await updateMediatorKeysHandler.Handle(addKeyRequest, CancellationToken.None);

        addKeyResult.IsSuccess.Should().BeTrue();
        addKeyResult.Value.ProblemReport.Should().BeNull();

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
        var response = await _httpClient.GetAsync(_prismMediatorUri + "invitationOOB");
        var resultContent = await response.Content.ReadAsStringAsync();
        var oob = resultContent.Split("=");
        var oobInvitation = oob[1];

        var secretResolverInMemory = new SecretResolverInMemory();
        var simpleDidDocResolver = new SimpleDidDocResolver();
        _createPeerDidHandler = new CreatePeerDidHandler(secretResolverInMemory);

        var localDid = await _createPeerDidHandler.Handle(new CreatePeerDidRequest(), cancellationToken: new CancellationToken());
        var request = new RequestMediationRequest(oobInvitation, localDid.Value.PeerDid.Value);

        _requestMediationHandler = new RequestMediationHandler(_httpClient, simpleDidDocResolver, secretResolverInMemory);
        var mediationResult = await _requestMediationHandler.Handle(request, CancellationToken.None);

        mediationResult.IsSuccess.Should().BeTrue();
        mediationResult.Value.MediationGranted.Should().BeTrue();
        mediationResult.Value.ProblemReport.Should().BeNull();

        var someTestKeysToAdd = await _createPeerDidHandler.Handle(new CreatePeerDidRequest(), cancellationToken: new CancellationToken());
        var addKeyRequest = new UpdateMediatorKeysRequest(mediationResult.Value.MediatorEndpoint, mediationResult.Value.MediatorDid, localDid.Value.PeerDid.Value, new List<string>() { someTestKeysToAdd.Value.PeerDid.Value }, new List<string>());
        var addMediatorKeysHandler = new UpdateMediatorKeysHandler(_httpClient, simpleDidDocResolver, secretResolverInMemory);
        var addKeyResult = await addMediatorKeysHandler.Handle(addKeyRequest, CancellationToken.None);

        addKeyResult.IsSuccess.Should().BeTrue();
        addKeyResult.Value.ProblemReport.Should().BeNull();

        // Act
        var queryKeysRequest = new QueryMediatorKeysRequest(mediationResult.Value.MediatorEndpoint, mediationResult.Value.MediatorDid, localDid.Value.PeerDid.Value);
        var queryMediatorKeysHandler = new QueryMediatorKeysHandler(_httpClient, simpleDidDocResolver, secretResolverInMemory);
        var queryKeyResult = await queryMediatorKeysHandler.Handle(queryKeysRequest, CancellationToken.None);

        // Assert
        queryKeyResult.IsSuccess.Should().BeTrue();
        queryKeyResult.Value.RegisteredMediatorKeys.Should().NotBeNull();
        queryKeyResult.Value.RegisteredMediatorKeys.Count.Should().Be(1);
        queryKeyResult.Value.RegisteredMediatorKeys[0].Should().Be(someTestKeysToAdd.Value.PeerDid.Value);
        queryKeyResult.Value.ProblemReport.Should().BeNull();
    }

    /// <summary>
    /// This tests assumes that the PRISM Mediator is running on https://beta-mediator.atalaprism.io
    /// </summary>
    [Fact]
    public async Task AddKeyToNonExistingConnectionCausesProblemReportAsResponse()
    {
        // Arrange
        var response = await _httpClient.GetAsync(_prismMediatorUri + "invitationOOB");
        var resultContent = await response.Content.ReadAsStringAsync();
        var oob = resultContent.Split("=");
        var oobInvitation = oob[1];

        var secretResolverInMemory = new SecretResolverInMemory();
        var simpleDidDocResolver = new SimpleDidDocResolver();
        _createPeerDidHandler = new CreatePeerDidHandler(secretResolverInMemory);

        var localDid = await _createPeerDidHandler.Handle(new CreatePeerDidRequest(), cancellationToken: new CancellationToken());
        var request = new RequestMediationRequest(oobInvitation, localDid.Value.PeerDid.Value);

        _requestMediationHandler = new RequestMediationHandler(_httpClient, simpleDidDocResolver, secretResolverInMemory);
        var mediationResult = await _requestMediationHandler.Handle(request, CancellationToken.None);

        mediationResult.IsSuccess.Should().BeTrue();
        mediationResult.Value.MediationGranted.Should().BeTrue();
        mediationResult.Value.ProblemReport.Should().BeNull();

        var someOtherNonRegisteredDid = await _createPeerDidHandler.Handle(new CreatePeerDidRequest(), cancellationToken: new CancellationToken());

        var someTestKeysToAdd = await _createPeerDidHandler.Handle(new CreatePeerDidRequest(), cancellationToken: new CancellationToken());
        var addKeyRequest = new UpdateMediatorKeysRequest(mediationResult.Value.MediatorEndpoint, mediationResult.Value.MediatorDid, someOtherNonRegisteredDid.Value.PeerDid.Value, new List<string>() { someTestKeysToAdd.Value.PeerDid.Value }, new List<string>());
        var addMediatorKeysHandler = new UpdateMediatorKeysHandler(_httpClient, simpleDidDocResolver, secretResolverInMemory);
        var addKeyResult = await addMediatorKeysHandler.Handle(addKeyRequest, CancellationToken.None);

        // Assert
        addKeyResult.IsSuccess.Should().BeTrue();
        addKeyResult.Value.ProblemReport.Should().NotBeNull();
    }

    /// <summary>
    /// This tests assumes that the PRISM Mediator is running on https://beta-mediator.atalaprism.io
    /// </summary>
    ///  THIS TEST FAILS AS OF October 04, 2023
    [Fact]
    public async Task QueryForNonExistingKeyShouldCauseErrorReport()
    {
        // Arrange
        var response = await _httpClient.GetAsync(_prismMediatorUri + "invitationOOB");
        var resultContent = await response.Content.ReadAsStringAsync();
        var oob = resultContent.Split("=");
        var oobInvitation = oob[1];

        var secretResolverInMemory = new SecretResolverInMemory();
        var simpleDidDocResolver = new SimpleDidDocResolver();
        _createPeerDidHandler = new CreatePeerDidHandler(secretResolverInMemory);

        var localDid = await _createPeerDidHandler.Handle(new CreatePeerDidRequest(), cancellationToken: new CancellationToken());
        var request = new RequestMediationRequest(oobInvitation, localDid.Value.PeerDid.Value);

        _requestMediationHandler = new RequestMediationHandler(_httpClient, simpleDidDocResolver, secretResolverInMemory);
        var mediationResult = await _requestMediationHandler.Handle(request, CancellationToken.None);

        mediationResult.IsSuccess.Should().BeTrue();
        mediationResult.Value.MediationGranted.Should().BeTrue();
        mediationResult.Value.ProblemReport.Should().BeNull();

        var someTestKeysToAdd = await _createPeerDidHandler.Handle(new CreatePeerDidRequest(), cancellationToken: new CancellationToken());
        var addKeyRequest = new UpdateMediatorKeysRequest(mediationResult.Value.MediatorEndpoint, mediationResult.Value.MediatorDid, localDid.Value.PeerDid.Value, new List<string>() { someTestKeysToAdd.Value.PeerDid.Value }, new List<string>());
        var addMediatorKeysHandler = new UpdateMediatorKeysHandler(_httpClient, simpleDidDocResolver, secretResolverInMemory);
        var addKeyResult = await addMediatorKeysHandler.Handle(addKeyRequest, CancellationToken.None);

        addKeyResult.IsSuccess.Should().BeTrue();
        addKeyResult.Value.ProblemReport.Should().BeNull();

        var someOtherNonRegisteredDid = await _createPeerDidHandler.Handle(new CreatePeerDidRequest(), cancellationToken: new CancellationToken());

        // Act
        var queryKeysRequest = new QueryMediatorKeysRequest(mediationResult.Value.MediatorEndpoint, mediationResult.Value.MediatorDid, someOtherNonRegisteredDid.Value.PeerDid.Value);
        var queryMediatorKeysHandler = new QueryMediatorKeysHandler(_httpClient, simpleDidDocResolver, secretResolverInMemory);
        var queryKeyResult = await queryMediatorKeysHandler.Handle(queryKeysRequest, CancellationToken.None);

        // Assert
        queryKeyResult.IsSuccess.Should().BeTrue();
        queryKeyResult.Value.ProblemReport.Should().NotBeNull();
    }
}