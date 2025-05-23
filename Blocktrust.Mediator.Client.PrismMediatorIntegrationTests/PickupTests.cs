﻿namespace Blocktrust.Mediator.Client.PrismMediatorIntegrationTests;

using Blocktrust.DIDComm.Secrets;
using Blocktrust.Mediator.Client.Commands.ForwardMessage;
using Blocktrust.Mediator.Client.Commands.MediatorCoordinator.RequestMediation;
using Blocktrust.Mediator.Client.Commands.MediatorCoordinator.UpdateKeys;
using Blocktrust.Mediator.Client.Commands.Pickup.DeliveryRequest;
using Blocktrust.Mediator.Client.Commands.Pickup.LiveDeliveryChange;
using Blocktrust.Mediator.Client.Commands.Pickup.MessageReceived;
using Blocktrust.Mediator.Client.Commands.Pickup.StatusRequest;
using Blocktrust.Mediator.Common;
using Blocktrust.Mediator.Common.Commands.CreatePeerDid;
using Blocktrust.Mediator.Common.Protocols;
using Blocktrust.PeerDID.DIDDoc;
using Blocktrust.PeerDID.PeerDIDCreateResolve;
using Blocktrust.PeerDID.Types;
using FluentAssertions;
using Xunit;

public class PickupTests
{
    private readonly HttpClient _httpClient;
    private RequestMediationHandler _requestMediationHandler;
    private CreatePeerDidHandler _createPeerDidHandlerAlice;
    private CreatePeerDidHandler _createPeerDidHandlerBob;
    private readonly string _prismMediatorUri = "https://beta-mediator.atalaprism.io/";
    private SendForwardMessageHandler _sendForwardMessageHandler;
    private StatusRequestHandler _statusRequestHandler;
    private DeliveryRequestHandler _deliveryRequestHandler;
    private MessageReceivedHandler _messageReceivedHandler;

    public PickupTests()
    {
        _httpClient = new HttpClient();
    }

    /// <summary>
    /// This tests assumes that a PRISM Mediator is running on https://beta-mediator.atalaprism.io
    /// </summary>
    [Fact]
    public async Task BobSendsBasicMessageToAliceAndAliceGetsStatusForAllDids()
    {
        // First get the OOB from the running mediator
        var response = await _httpClient.GetAsync(_prismMediatorUri + "invitationOOB");
        var resultContent = await response.Content.ReadAsStringAsync();
        var oob = resultContent.Split("=");
        var oobInvitation = oob[1];

        var secretResolverInMemoryForAlice = new SecretResolverInMemory();
        var simpleDidDocResolverForAlice = new SimpleDidDocResolver();
        _createPeerDidHandlerAlice = new CreatePeerDidHandler(secretResolverInMemoryForAlice);

        var localDidOfAliceToUseWithTheMediator = await _createPeerDidHandlerAlice.Handle(new CreatePeerDidRequest(), cancellationToken: new CancellationToken());
        var request = new RequestMediationRequest(oobInvitation, localDidOfAliceToUseWithTheMediator.Value.PeerDid.Value);

        _requestMediationHandler = new RequestMediationHandler(_httpClient, simpleDidDocResolverForAlice, secretResolverInMemoryForAlice);
        var requestMediationResult = await _requestMediationHandler.Handle(request, CancellationToken.None);

        // Alice create now an additional DID to be used with Bob. Important: The service endpoint of the DID must be set to the mediator endpoint
        var localDidOfAliceToUseWithBob = await _createPeerDidHandlerAlice.Handle(new CreatePeerDidRequest(serviceEndpointDid: requestMediationResult.Value.RoutingDid), cancellationToken: new CancellationToken());

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
        var basicMessage = BasicMessage.Create("Hello Alice", localDidOfBobToUseWithAlice.Value.PeerDid.Value);
        var packedBasicMessage = await BasicMessage.Pack(basicMessage, from: localDidOfBobToUseWithAlice.Value.PeerDid.Value, localDidOfAliceToUseWithBob.Value.PeerDid.Value, secretResolverInMemoryForBob, simpleDidDocResolverForBob);

        // Bob creates a DID just to be used with the mediator
        var localDidOfBobToUseWithAliceMediator = await _createPeerDidHandlerBob.Handle(new CreatePeerDidRequest(), cancellationToken: new CancellationToken());

        // Wrap the Basic Message into a new Message for the mediator to recieve and send it
        var resolvedMediatorDid = PeerDidResolver.ResolvePeerDid(new PeerDid(localDidOfAliceToUseWithBob.Value.DidDoc.Services.First().ServiceEndpoint.Uri), VerificationMaterialFormatPeerDid.Jwk);
        var resolvedMediatorDidDoc = DidDocPeerDid.FromJson(resolvedMediatorDid.Value);
        var resolvedMediatorDidEndpoint = resolvedMediatorDidDoc.Value.Services.First().ServiceEndpoint;

        _sendForwardMessageHandler = new SendForwardMessageHandler(_httpClient, simpleDidDocResolverForBob, secretResolverInMemoryForBob);
        var result = await _sendForwardMessageHandler.Handle(new SendForwardMessageRequest(
            message: packedBasicMessage.Value,
            localDid: localDidOfBobToUseWithAliceMediator.Value.PeerDid.Value,
            mediatorDid: localDidOfAliceToUseWithBob.Value.DidDoc.Services.First().ServiceEndpoint.Uri,
            mediatorEndpoint: new Uri(resolvedMediatorDidEndpoint.Uri),
            recipientDid: localDidOfAliceToUseWithBob.Value.PeerDid.Value
        ), new CancellationToken());

        // Alice asks the Mediator for new Messages 
        _statusRequestHandler = new StatusRequestHandler(_httpClient, simpleDidDocResolverForAlice, secretResolverInMemoryForAlice);
        var statusRequestResult = await _statusRequestHandler.Handle(new StatusRequestRequest(localDidOfAliceToUseWithTheMediator.Value.PeerDid.Value, requestMediationResult.Value.MediatorDid, requestMediationResult.Value.MediatorEndpoint, null), new CancellationToken());

        // Assert
        statusRequestResult.IsSuccess.Should().BeTrue();
        statusRequestResult.Value.MessageCount.Should().Be(1);
    }

    /// <summary>
    /// This tests assumes that a PRISM Mediator is running on https://beta-mediator.atalaprism.io
    /// </summary>
    [Fact]
    public async Task BobSendsBasicMessageToAliceAndAliceGetsStatusForSpecificDid()
    {
        // First get the OOB from the running mediator
        var response = await _httpClient.GetAsync(_prismMediatorUri + "invitationOOB");
        var resultContent = await response.Content.ReadAsStringAsync();
        var oob = resultContent.Split("=");
        var oobInvitation = oob[1];

        var secretResolverInMemoryForAlice = new SecretResolverInMemory();
        var simpleDidDocResolverForAlice = new SimpleDidDocResolver();
        _createPeerDidHandlerAlice = new CreatePeerDidHandler(secretResolverInMemoryForAlice);

        var localDidOfAliceToUseWithTheMediator = await _createPeerDidHandlerAlice.Handle(new CreatePeerDidRequest(), cancellationToken: new CancellationToken());
        var request = new RequestMediationRequest(oobInvitation, localDidOfAliceToUseWithTheMediator.Value.PeerDid.Value);

        _requestMediationHandler = new RequestMediationHandler(_httpClient, simpleDidDocResolverForAlice, secretResolverInMemoryForAlice);
        var requestMediationResult = await _requestMediationHandler.Handle(request, CancellationToken.None);

        // Alice create now an additional DID to be used with Bob. Important: The service endpoint of the DID must be set to the mediator endpoint
        var localDidOfAliceToUseWithBob = await _createPeerDidHandlerAlice.Handle(new CreatePeerDidRequest(requestMediationResult.Value.RoutingDid), cancellationToken: new CancellationToken());

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
        var basicMessage = BasicMessage.Create("Hello Alice", localDidOfBobToUseWithAlice.Value.PeerDid.Value);
        var packedBasicMessage = await BasicMessage.Pack(basicMessage, from: localDidOfBobToUseWithAlice.Value.PeerDid.Value, localDidOfAliceToUseWithBob.Value.PeerDid.Value, secretResolverInMemoryForBob, simpleDidDocResolverForBob);

        // Bob creates a DID just to be used with the mediator
        var localDidOfBobToUseWithAliceMediator = await _createPeerDidHandlerBob.Handle(new CreatePeerDidRequest(), cancellationToken: new CancellationToken());

        // Wrap the Basic Message into a new Message for the mediator to recieve and send it
        var resolvedMediatorDid = PeerDidResolver.ResolvePeerDid(new PeerDid(localDidOfAliceToUseWithBob.Value.DidDoc.Services.First().ServiceEndpoint.Uri), VerificationMaterialFormatPeerDid.Jwk);
        var resolvedMediatorDidDoc = DidDocPeerDid.FromJson(resolvedMediatorDid.Value);
        var resolvedMediatorDidEndpoint = resolvedMediatorDidDoc.Value.Services.First().ServiceEndpoint;

        _sendForwardMessageHandler = new SendForwardMessageHandler(_httpClient, simpleDidDocResolverForBob, secretResolverInMemoryForBob);
        var result = await _sendForwardMessageHandler.Handle(new SendForwardMessageRequest(
            message: packedBasicMessage.Value,
            localDid: localDidOfBobToUseWithAliceMediator.Value.PeerDid.Value,
            mediatorDid: localDidOfAliceToUseWithBob.Value.DidDoc.Services.First().ServiceEndpoint.Uri, // The mediator DID was also shared beforehand (should be in the shared DID of alice)
            mediatorEndpoint: new Uri(resolvedMediatorDidEndpoint.Uri),
            recipientDid: localDidOfAliceToUseWithBob.Value.PeerDid.Value
        ), new CancellationToken());

        // Alice asks the Mediator for new Messages 
        _statusRequestHandler = new StatusRequestHandler(_httpClient, simpleDidDocResolverForAlice, secretResolverInMemoryForAlice);
        var didToCheckSpecifically = localDidOfAliceToUseWithBob.Value.PeerDid.Value;
        var statusRequestResult = await _statusRequestHandler.Handle(new StatusRequestRequest(localDidOfAliceToUseWithTheMediator.Value.PeerDid.Value, requestMediationResult.Value.MediatorDid, requestMediationResult.Value.MediatorEndpoint, didToCheckSpecifically), new CancellationToken());

        // Assert
        statusRequestResult.IsSuccess.Should().BeTrue();
        statusRequestResult.Value.MessageCount.Should().Be(1);
        // TODO Currently fails for the roots mediator, but works here
        statusRequestResult.Value.RecipientDid.Should().Be(didToCheckSpecifically);
    }

    /// <summary>
    /// This tests assumes that a PRISM Mediator is running on https://beta-mediator.atalaprism.io
    /// </summary>
    [Fact]
    public async Task BobSendsTwoBasicMessageToAliceAndAliceGetsStatusForSpecificDid()
    {
        // First get the OOB from the running mediator
        var response = await _httpClient.GetAsync(_prismMediatorUri + "invitationOOB");
        var resultContent = await response.Content.ReadAsStringAsync();
        var oob = resultContent.Split("=");
        var oobInvitation = oob[1];

        var secretResolverInMemoryForAlice = new SecretResolverInMemory();
        var simpleDidDocResolverForAlice = new SimpleDidDocResolver();
        _createPeerDidHandlerAlice = new CreatePeerDidHandler(secretResolverInMemoryForAlice);

        var localDidOfAliceToUseWithTheMediator = await _createPeerDidHandlerAlice.Handle(new CreatePeerDidRequest(), cancellationToken: new CancellationToken());
        var request = new RequestMediationRequest(oobInvitation, localDidOfAliceToUseWithTheMediator.Value.PeerDid.Value);

        _requestMediationHandler = new RequestMediationHandler(_httpClient, simpleDidDocResolverForAlice, secretResolverInMemoryForAlice);
        var requestMediationResult = await _requestMediationHandler.Handle(request, CancellationToken.None);

        // Alice create now an additional DID to be used with Bob. Important: The service endpoint of the DID must be set to the mediator endpoint
        var localDidOfAliceToUseWithBob = await _createPeerDidHandlerAlice.Handle(new CreatePeerDidRequest(requestMediationResult.Value.RoutingDid), cancellationToken: new CancellationToken());

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
        var basicMessage1 = BasicMessage.Create("Hello Alice", localDidOfBobToUseWithAlice.Value.PeerDid.Value);
        var packedBasicMessage1 = await BasicMessage.Pack(basicMessage1, from: localDidOfBobToUseWithAlice.Value.PeerDid.Value, localDidOfAliceToUseWithBob.Value.PeerDid.Value, secretResolverInMemoryForBob, simpleDidDocResolverForBob);
        // Bob also creates a second "Basic Message"
        var basicMessage2 = BasicMessage.Create("How are you Alice?", localDidOfBobToUseWithAlice.Value.PeerDid.Value);
        var packedBasicMessage2 = await BasicMessage.Pack(basicMessage2, from: localDidOfBobToUseWithAlice.Value.PeerDid.Value, localDidOfAliceToUseWithBob.Value.PeerDid.Value, secretResolverInMemoryForBob, simpleDidDocResolverForBob);

        // Bob creates a DID just to be used with the mediator
        var localDidOfBobToUseWithAliceMediator = await _createPeerDidHandlerBob.Handle(new CreatePeerDidRequest(), cancellationToken: new CancellationToken());

        // Wrap the Basic Message into a new Message for the mediator to recieve and send it
        var resolvedMediatorDid = PeerDidResolver.ResolvePeerDid(new PeerDid(localDidOfAliceToUseWithBob.Value.DidDoc.Services.First().ServiceEndpoint.Uri), VerificationMaterialFormatPeerDid.Jwk);
        var resolvedMediatorDidDoc = DidDocPeerDid.FromJson(resolvedMediatorDid.Value);
        var resolvedMediatorDidEndpoint = resolvedMediatorDidDoc.Value.Services.First().ServiceEndpoint;
        _sendForwardMessageHandler = new SendForwardMessageHandler(_httpClient, simpleDidDocResolverForBob, secretResolverInMemoryForBob);
        var result1 = await _sendForwardMessageHandler.Handle(new SendForwardMessageRequest(
            message: packedBasicMessage1.Value,
            localDid: localDidOfBobToUseWithAliceMediator.Value.PeerDid.Value,
            mediatorDid: localDidOfAliceToUseWithBob.Value.DidDoc.Services.First().ServiceEndpoint.Uri, // The mediator DID was also shared beforehand (should be in the shared DID of alice)
            mediatorEndpoint: new Uri(resolvedMediatorDidEndpoint.Uri),
            recipientDid: localDidOfAliceToUseWithBob.Value.PeerDid.Value
        ), new CancellationToken());
        result1.IsSuccess.Should().BeTrue();
        var result2 = await _sendForwardMessageHandler.Handle(new SendForwardMessageRequest(
            message: packedBasicMessage2.Value,
            localDid: localDidOfBobToUseWithAliceMediator.Value.PeerDid.Value,
            mediatorDid: localDidOfAliceToUseWithBob.Value.DidDoc.Services.First().ServiceEndpoint.Uri, // The mediator DID was also shared beforehand (should be in the shared DID of alice)
            mediatorEndpoint: new Uri(resolvedMediatorDidEndpoint.Uri),
            recipientDid: localDidOfAliceToUseWithBob.Value.PeerDid.Value
        ), new CancellationToken());
        result2.IsSuccess.Should().BeTrue();

        // Alice asks the Mediator for new Messages 
        _statusRequestHandler = new StatusRequestHandler(_httpClient, simpleDidDocResolverForAlice, secretResolverInMemoryForAlice);
        var didToCheckSpecifically = localDidOfAliceToUseWithBob.Value.PeerDid.Value;
        var statusRequestResult = await _statusRequestHandler.Handle(new StatusRequestRequest(localDidOfAliceToUseWithTheMediator.Value.PeerDid.Value, requestMediationResult.Value.MediatorDid, requestMediationResult.Value.MediatorEndpoint, didToCheckSpecifically), new CancellationToken());

        // Assert
        statusRequestResult.IsSuccess.Should().BeTrue();
        statusRequestResult.Value.MessageCount.Should().Be(2);
        statusRequestResult.Value.RecipientDid.Should().Be(didToCheckSpecifically);
    }

    /// <summary>
    /// This tests assumes that a PRISM Mediator is running on https://beta-mediator.atalaprism.io
    /// </summary>
    [Fact]
    public async Task BobSendsBasicMessageToAliceAndAliceGetsTheMessageFromTheMediator()
    {
        // First get the OOB from the running mediator
        var response = await _httpClient.GetAsync(_prismMediatorUri + "invitationOOB");
        var resultContent = await response.Content.ReadAsStringAsync();
        var oob = resultContent.Split("=");
        var oobInvitation = oob[1];

        var secretResolverInMemoryForAlice = new SecretResolverInMemory();
        var simpleDidDocResolverForAlice = new SimpleDidDocResolver();
        _createPeerDidHandlerAlice = new CreatePeerDidHandler(secretResolverInMemoryForAlice);

        var localDidOfAliceToUseWithTheMediator = await _createPeerDidHandlerAlice.Handle(new CreatePeerDidRequest(), cancellationToken: new CancellationToken());
        var request = new RequestMediationRequest(oobInvitation, localDidOfAliceToUseWithTheMediator.Value.PeerDid.Value);

        _requestMediationHandler = new RequestMediationHandler(_httpClient, simpleDidDocResolverForAlice, secretResolverInMemoryForAlice);
        var requestMediationResult = await _requestMediationHandler.Handle(request, CancellationToken.None);

        // Alice create now an additional DID to be used with Bob. Important: The service endpoint of the DID must be set to the mediator endpoint
        var localDidOfAliceToUseWithBob = await _createPeerDidHandlerAlice.Handle(new CreatePeerDidRequest(requestMediationResult.Value.RoutingDid), cancellationToken: new CancellationToken());

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
        var basicMessage = BasicMessage.Create("Hello Alice", localDidOfBobToUseWithAlice.Value.PeerDid.Value);
        var packedBasicMessage = await BasicMessage.Pack(basicMessage, from: localDidOfBobToUseWithAlice.Value.PeerDid.Value, localDidOfAliceToUseWithBob.Value.PeerDid.Value, secretResolverInMemoryForBob, simpleDidDocResolverForBob);

        // Bob creates a DID just to be used with the mediator
        var localDidOfBobToUseWithAliceMediator = await _createPeerDidHandlerBob.Handle(new CreatePeerDidRequest(), cancellationToken: new CancellationToken());

        // Wrap the Basic Message into a new Message for the mediator to recieve and send it
        var resolvedMediatorDid = PeerDidResolver.ResolvePeerDid(new PeerDid(localDidOfAliceToUseWithBob.Value.DidDoc.Services.First().ServiceEndpoint.Uri), VerificationMaterialFormatPeerDid.Jwk);
        var resolvedMediatorDidDoc = DidDocPeerDid.FromJson(resolvedMediatorDid.Value);
        var resolvedMediatorDidEndpoint = resolvedMediatorDidDoc.Value.Services.First().ServiceEndpoint;
        _sendForwardMessageHandler = new SendForwardMessageHandler(_httpClient, simpleDidDocResolverForBob, secretResolverInMemoryForBob);
        var result = await _sendForwardMessageHandler.Handle(new SendForwardMessageRequest(
            message: packedBasicMessage.Value,
            localDid: localDidOfBobToUseWithAliceMediator.Value.PeerDid.Value,
            mediatorDid: localDidOfAliceToUseWithBob.Value.DidDoc.Services.First().ServiceEndpoint.Uri, // The mediator DID was also shared beforehand (should be in the shared DID of alice)
            mediatorEndpoint: new Uri(resolvedMediatorDidEndpoint.Uri),
            recipientDid: localDidOfAliceToUseWithBob.Value.PeerDid.Value
        ), new CancellationToken());

        // Alice asks the Mediator for new Messages 
        _deliveryRequestHandler = new DeliveryRequestHandler(_httpClient, simpleDidDocResolverForAlice, secretResolverInMemoryForAlice);
        var limit = 100; // We currently don't process this limit, we just ignore it on the mediator. We need a test for that
        var deliveryRequestResult = await _deliveryRequestHandler.Handle(new DeliveryRequestRequest(localDidOfAliceToUseWithTheMediator.Value.PeerDid.Value, requestMediationResult.Value.MediatorDid, requestMediationResult.Value.MediatorEndpoint, limit), new CancellationToken());

        // Assert
        deliveryRequestResult.IsSuccess.Should().BeTrue();
        deliveryRequestResult.Value.Messages!.Count.Should().Be(1);
        var basicMessageResult = BasicMessage.Parse(deliveryRequestResult.Value.Messages[0]);
        basicMessageResult.IsSuccess.Should().BeTrue();
        basicMessageResult.Value.Message.Should().Be("Hello Alice");
        basicMessageResult.Value.From.Should().Be(localDidOfBobToUseWithAlice.Value.PeerDid.Value);
        basicMessageResult.Value.Tos.FirstOrDefault().Should().Be(localDidOfAliceToUseWithBob.Value.PeerDid.Value);
    }

    /// <summary>
    /// This tests assumes that a PRISM Mediator is running on https://beta-mediator.atalaprism.io
    /// </summary>
    [Fact]
    public async Task BobSendsBasicMessageToAliceAndAliceGetsTheMessageFromTheMediatorForASpecificDid()
    {
        // First get the OOB from the running mediator
        var response = await _httpClient.GetAsync(_prismMediatorUri + "invitationOOB");
        var resultContent = await response.Content.ReadAsStringAsync();
        var oob = resultContent.Split("=");
        var oobInvitation = oob[1];

        var secretResolverInMemoryForAlice = new SecretResolverInMemory();
        var simpleDidDocResolverForAlice = new SimpleDidDocResolver();
        _createPeerDidHandlerAlice = new CreatePeerDidHandler(secretResolverInMemoryForAlice);

        var localDidOfAliceToUseWithTheMediator = await _createPeerDidHandlerAlice.Handle(new CreatePeerDidRequest(), cancellationToken: new CancellationToken());
        var request = new RequestMediationRequest(oobInvitation, localDidOfAliceToUseWithTheMediator.Value.PeerDid.Value);

        _requestMediationHandler = new RequestMediationHandler(_httpClient, simpleDidDocResolverForAlice, secretResolverInMemoryForAlice);
        var requestMediationResult = await _requestMediationHandler.Handle(request, CancellationToken.None);

        // Alice create now an additional DID to be used with Bob. Important: The service endpoint of the DID must be set to the mediator endpoint
        var localDidOfAliceToUseWithBob = await _createPeerDidHandlerAlice.Handle(new CreatePeerDidRequest(requestMediationResult.Value.RoutingDid), cancellationToken: new CancellationToken());

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
        var basicMessage = BasicMessage.Create("Hello Alice", localDidOfBobToUseWithAlice.Value.PeerDid.Value);
        var packedBasicMessage = await BasicMessage.Pack(basicMessage, from: localDidOfBobToUseWithAlice.Value.PeerDid.Value, localDidOfAliceToUseWithBob.Value.PeerDid.Value, secretResolverInMemoryForBob, simpleDidDocResolverForBob);

        // Bob creates a DID just to be used with the mediator
        var localDidOfBobToUseWithAliceMediator = await _createPeerDidHandlerBob.Handle(new CreatePeerDidRequest(), cancellationToken: new CancellationToken());

        // Wrap the Basic Message into a new Message for the mediator to recieve and send it
        var resolvedMediatorDid = PeerDidResolver.ResolvePeerDid(new PeerDid(localDidOfAliceToUseWithBob.Value.DidDoc.Services.First().ServiceEndpoint.Uri), VerificationMaterialFormatPeerDid.Jwk);
        var resolvedMediatorDidDoc = DidDocPeerDid.FromJson(resolvedMediatorDid.Value);
        var resolvedMediatorDidEndpoint = resolvedMediatorDidDoc.Value.Services.First().ServiceEndpoint;
        _sendForwardMessageHandler = new SendForwardMessageHandler(_httpClient, simpleDidDocResolverForBob, secretResolverInMemoryForBob);
        var result = await _sendForwardMessageHandler.Handle(new SendForwardMessageRequest(
            message: packedBasicMessage.Value,
            localDid: localDidOfBobToUseWithAliceMediator.Value.PeerDid.Value,
            mediatorDid: localDidOfAliceToUseWithBob.Value.DidDoc.Services.First().ServiceEndpoint.Uri, // The mediator DID was also shared beforehand (should be in the shared DID of alice)
            mediatorEndpoint: new Uri(resolvedMediatorDidEndpoint.Uri),
            recipientDid: localDidOfAliceToUseWithBob.Value.PeerDid.Value
        ), new CancellationToken());

        // Alice asks the Mediator for new Messages 
        _deliveryRequestHandler = new DeliveryRequestHandler(_httpClient, simpleDidDocResolverForAlice, secretResolverInMemoryForAlice);
        var limit = 100;
        var deliveryRequestResult = await _deliveryRequestHandler.Handle(
            new DeliveryRequestRequest(localDidOfAliceToUseWithTheMediator.Value.PeerDid.Value, requestMediationResult.Value.MediatorDid, requestMediationResult.Value.MediatorEndpoint, limit, localDidOfAliceToUseWithBob.Value.PeerDid.Value), new CancellationToken());

        // Assert
        deliveryRequestResult.IsSuccess.Should().BeTrue();
        deliveryRequestResult.Value.Messages!.Count.Should().Be(1);
        var basicMessageResult = BasicMessage.Parse(deliveryRequestResult.Value.Messages[0]);
        basicMessageResult.IsSuccess.Should().BeTrue();
        basicMessageResult.Value.Message.Should().Be("Hello Alice");
        basicMessageResult.Value.From.Should().Be(localDidOfBobToUseWithAlice.Value.PeerDid.Value);
        basicMessageResult.Value.Tos.FirstOrDefault().Should().Be(localDidOfAliceToUseWithBob.Value.PeerDid.Value);
    }

    /// <summary>
    /// This tests assumes that a PRISM Mediator is running on https://beta-mediator.atalaprism.io
    /// </summary>
    [Fact]
    public async Task BobSendsBasicMessageToAliceAndAliceGetsTheMessageFromTheMediatorAndConfirmsDelivery()
    {
        // First get the OOB from the running mediator
        var response = await _httpClient.GetAsync(_prismMediatorUri + "invitationOOB");
        var resultContent = await response.Content.ReadAsStringAsync();
        var oob = resultContent.Split("=");
        var oobInvitation = oob[1];

        var secretResolverInMemoryForAlice = new SecretResolverInMemory();
        var simpleDidDocResolverForAlice = new SimpleDidDocResolver();
        _createPeerDidHandlerAlice = new CreatePeerDidHandler(secretResolverInMemoryForAlice);

        var localDidOfAliceToUseWithTheMediator = await _createPeerDidHandlerAlice.Handle(new CreatePeerDidRequest(), cancellationToken: new CancellationToken());
        var request = new RequestMediationRequest(oobInvitation, localDidOfAliceToUseWithTheMediator.Value.PeerDid.Value);

        _requestMediationHandler = new RequestMediationHandler(_httpClient, simpleDidDocResolverForAlice, secretResolverInMemoryForAlice);
        var requestMediationResult = await _requestMediationHandler.Handle(request, CancellationToken.None);

        // Alice create now an additional DID to be used with Bob. Important: The service endpoint of the DID must be set to the mediator endpoint
        var localDidOfAliceToUseWithBob = await _createPeerDidHandlerAlice.Handle(new CreatePeerDidRequest(requestMediationResult.Value.RoutingDid), cancellationToken: new CancellationToken());

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
        var basicMessage = BasicMessage.Create("Hello Alice", localDidOfBobToUseWithAlice.Value.PeerDid.Value);
        var packedBasicMessage = await BasicMessage.Pack(basicMessage, from: localDidOfBobToUseWithAlice.Value.PeerDid.Value, localDidOfAliceToUseWithBob.Value.PeerDid.Value, secretResolverInMemoryForBob, simpleDidDocResolverForBob);

        // Bob creates a DID just to be used with the mediator
        var localDidOfBobToUseWithAliceMediator = await _createPeerDidHandlerBob.Handle(new CreatePeerDidRequest(), cancellationToken: new CancellationToken());

        // Wrap the Basic Message into a new Message for the mediator to recieve and send it
        var resolvedMediatorDid = PeerDidResolver.ResolvePeerDid(new PeerDid(localDidOfAliceToUseWithBob.Value.DidDoc.Services.First().ServiceEndpoint.Uri), VerificationMaterialFormatPeerDid.Jwk);
        var resolvedMediatorDidDoc = DidDocPeerDid.FromJson(resolvedMediatorDid.Value);
        var resolvedMediatorDidEndpoint = resolvedMediatorDidDoc.Value.Services.First().ServiceEndpoint;
        _sendForwardMessageHandler = new SendForwardMessageHandler(_httpClient, simpleDidDocResolverForBob, secretResolverInMemoryForBob);
        var result = await _sendForwardMessageHandler.Handle(new SendForwardMessageRequest(
            message: packedBasicMessage.Value,
            localDid: localDidOfBobToUseWithAliceMediator.Value.PeerDid.Value,
            mediatorDid: localDidOfAliceToUseWithBob.Value.DidDoc.Services.First().ServiceEndpoint.Uri, // The mediator DID was also shared beforehand (should be in the shared DID of alice)
            mediatorEndpoint: new Uri(resolvedMediatorDidEndpoint.Uri),
            recipientDid: localDidOfAliceToUseWithBob.Value.PeerDid.Value
        ), new CancellationToken());

        // Alice asks the Mediator for new Messages 
        _deliveryRequestHandler = new DeliveryRequestHandler(_httpClient, simpleDidDocResolverForAlice, secretResolverInMemoryForAlice);
        var limit = 100;
        var deliveryRequestResult = await _deliveryRequestHandler.Handle(new DeliveryRequestRequest(localDidOfAliceToUseWithTheMediator.Value.PeerDid.Value, requestMediationResult.Value.MediatorDid, requestMediationResult.Value.MediatorEndpoint, limit), new CancellationToken());

        // Alice confirms the delivery of the message
        var messageId = deliveryRequestResult.Value.Messages!.FirstOrDefault().MessageId;
        _messageReceivedHandler = new MessageReceivedHandler(_httpClient, simpleDidDocResolverForAlice, secretResolverInMemoryForAlice);
        var messageReceivedResult = await _messageReceivedHandler.Handle(new MessageReceivedRequest(localDidOfAliceToUseWithTheMediator.Value.PeerDid.Value, requestMediationResult.Value.MediatorDid, requestMediationResult.Value.MediatorEndpoint, new List<string>() { messageId }),
            new CancellationToken());

        // Assert
        messageReceivedResult.IsSuccess.Should().BeTrue();
        messageReceivedResult.Value.MessageCount.Should().Be(0);
    }

    /// <summary>
    /// This tests assumes that a PRISM Mediator is running on https://beta-mediator.atalaprism.io
    /// </summary>
    [Fact]
    public async Task BobSendsBasicMessageToAliceAndAliceGetsTheMessageFromTheMediatorAndDeliveryConfirmationFailsBecauseOfUnknownDidWithProblemReport()
    {
        // First get the OOB from the running mediator
        var response = await _httpClient.GetAsync(_prismMediatorUri + "invitationOOB");
        var resultContent = await response.Content.ReadAsStringAsync();
        var oob = resultContent.Split("=");
        var oobInvitation = oob[1];

        var secretResolverInMemoryForAlice = new SecretResolverInMemory();
        var simpleDidDocResolverForAlice = new SimpleDidDocResolver();
        _createPeerDidHandlerAlice = new CreatePeerDidHandler(secretResolverInMemoryForAlice);

        var localDidOfAliceToUseWithTheMediator = await _createPeerDidHandlerAlice.Handle(new CreatePeerDidRequest(), cancellationToken: new CancellationToken());
        var request = new RequestMediationRequest(oobInvitation, localDidOfAliceToUseWithTheMediator.Value.PeerDid.Value);

        _requestMediationHandler = new RequestMediationHandler(_httpClient, simpleDidDocResolverForAlice, secretResolverInMemoryForAlice);
        var requestMediationResult = await _requestMediationHandler.Handle(request, CancellationToken.None);

        // Alice create now an additional DID to be used with Bob. Important: The service endpoint of the DID must be set to the mediator endpoint
        var localDidOfAliceToUseWithBob = await _createPeerDidHandlerAlice.Handle(new CreatePeerDidRequest(requestMediationResult.Value.RoutingDid), cancellationToken: new CancellationToken());

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
        var basicMessage = BasicMessage.Create("Hello Alice", localDidOfBobToUseWithAlice.Value.PeerDid.Value);
        var packedBasicMessage = await BasicMessage.Pack(basicMessage, from: localDidOfBobToUseWithAlice.Value.PeerDid.Value, localDidOfAliceToUseWithBob.Value.PeerDid.Value, secretResolverInMemoryForBob, simpleDidDocResolverForBob);

        // Bob creates a DID just to be used with the mediator
        var localDidOfBobToUseWithAliceMediator = await _createPeerDidHandlerBob.Handle(new CreatePeerDidRequest(), cancellationToken: new CancellationToken());

        // Wrap the Basic Message into a new Message for the mediator to recieve and send it
        var resolvedMediatorDid = PeerDidResolver.ResolvePeerDid(new PeerDid(localDidOfAliceToUseWithBob.Value.DidDoc.Services.First().ServiceEndpoint.Uri), VerificationMaterialFormatPeerDid.Jwk);
        var resolvedMediatorDidDoc = DidDocPeerDid.FromJson(resolvedMediatorDid.Value);
        var resolvedMediatorDidEndpoint = resolvedMediatorDidDoc.Value.Services.First().ServiceEndpoint;
        _sendForwardMessageHandler = new SendForwardMessageHandler(_httpClient, simpleDidDocResolverForBob, secretResolverInMemoryForBob);
        var result = await _sendForwardMessageHandler.Handle(new SendForwardMessageRequest(
            message: packedBasicMessage.Value,
            localDid: localDidOfBobToUseWithAliceMediator.Value.PeerDid.Value,
            mediatorDid: localDidOfAliceToUseWithBob.Value.DidDoc.Services.First().ServiceEndpoint.Uri, // The mediator DID was also shared beforehand (should be in the shared DID of alice)
            mediatorEndpoint: new Uri(resolvedMediatorDidEndpoint.Uri),
            recipientDid: localDidOfAliceToUseWithBob.Value.PeerDid.Value
        ), new CancellationToken());

        // Alice asks the Mediator for new Messages 
        _deliveryRequestHandler = new DeliveryRequestHandler(_httpClient, simpleDidDocResolverForAlice, secretResolverInMemoryForAlice);
        var limit = 100;
        var deliveryRequestResult = await _deliveryRequestHandler.Handle(new DeliveryRequestRequest(localDidOfAliceToUseWithTheMediator.Value.PeerDid.Value, requestMediationResult.Value.MediatorDid, requestMediationResult.Value.MediatorEndpoint, limit), new CancellationToken());

        // Alice confirms the delivery of the message
        var messageId = deliveryRequestResult.Value.Messages!.FirstOrDefault().MessageId;
        _messageReceivedHandler = new MessageReceivedHandler(_httpClient, simpleDidDocResolverForAlice, secretResolverInMemoryForAlice);

        var unknownDid = await _createPeerDidHandlerAlice.Handle(new CreatePeerDidRequest(), new CancellationToken());

        var messageReceivedResult = await _messageReceivedHandler.Handle(new MessageReceivedRequest(unknownDid.Value.PeerDid.Value, requestMediationResult.Value.MediatorDid, requestMediationResult.Value.MediatorEndpoint, new List<string>() { messageId }),
            new CancellationToken());

        // Assert
        messageReceivedResult.IsSuccess.Should().BeTrue();
        messageReceivedResult.Value.ProblemReport.Should().NotBeNull();
    }

    /// <summary>
    /// This tests assumes that a PRISM Mediator is running on https://beta-mediator.atalaprism.io
    /// </summary>
    /// /// NOT PASSING THIS TEST AS OF October 4, 2023
    [Fact]
    public async Task AliceWantsToChangeTheDeliveryLiveMode()
    {
        // First get the OOB from the running mediator
        var response = await _httpClient.GetAsync(_prismMediatorUri + "invitationOOB");
        var resultContent = await response.Content.ReadAsStringAsync();
        var oob = resultContent.Split("=");
        var oobInvitation = oob[1];

        var secretResolverInMemoryForAlice = new SecretResolverInMemory();
        var simpleDidDocResolverForAlice = new SimpleDidDocResolver();
        _createPeerDidHandlerAlice = new CreatePeerDidHandler(secretResolverInMemoryForAlice);

        var localDidOfAliceToUseWithTheMediator = await _createPeerDidHandlerAlice.Handle(new CreatePeerDidRequest(), cancellationToken: new CancellationToken());
        var request = new RequestMediationRequest(oobInvitation, localDidOfAliceToUseWithTheMediator.Value.PeerDid.Value);

        _requestMediationHandler = new RequestMediationHandler(_httpClient, simpleDidDocResolverForAlice, secretResolverInMemoryForAlice);
        var requestMediationResult = await _requestMediationHandler.Handle(request, CancellationToken.None);

        var liveDeliveryChangeHandler = new LiveDeliveryChangeHandler(_httpClient, simpleDidDocResolverForAlice, secretResolverInMemoryForAlice);
        var result = await liveDeliveryChangeHandler.Handle(new LiveDeliveryChangeRequest(localDidOfAliceToUseWithTheMediator.Value.PeerDid.Value, requestMediationResult.Value.MediatorDid, requestMediationResult.Value.MediatorEndpoint, true), cancellationToken: new CancellationToken());

        result.IsSuccess.Should().BeTrue();
        result.Value.ProblemCode.ToString().Should().Be("e.m.live-delivery-not-supported");
        result.Value.Comment.Should().Be("Connection does not support Live Delivery");
    }

    /// <summary>
    /// This tests assumes that a PRISM Mediator is running on https://beta-mediator.atalaprism.io
    /// </summary>
    /// /// NOT PASSING THIS TEST AS OF October 4, 2023
    [Fact]
    public async Task AliceWantsToPickUpMessagesButNoMessagesWasRecived()
    {
        // First get the OOB from the running mediator
        var response = await _httpClient.GetAsync(_prismMediatorUri + "invitationOOB");
        var resultContent = await response.Content.ReadAsStringAsync();
        var oob = resultContent.Split("=");
        var oobInvitation = oob[1];

        var secretResolverInMemoryForAlice = new SecretResolverInMemory();
        var simpleDidDocResolverForAlice = new SimpleDidDocResolver();
        _createPeerDidHandlerAlice = new CreatePeerDidHandler(secretResolverInMemoryForAlice);

        var localDidOfAliceToUseWithTheMediator = await _createPeerDidHandlerAlice.Handle(new CreatePeerDidRequest(), cancellationToken: new CancellationToken());
        var request = new RequestMediationRequest(oobInvitation, localDidOfAliceToUseWithTheMediator.Value.PeerDid.Value);

        _requestMediationHandler = new RequestMediationHandler(_httpClient, simpleDidDocResolverForAlice, secretResolverInMemoryForAlice);
        var requestMediationResult = await _requestMediationHandler.Handle(request, CancellationToken.None);

        _deliveryRequestHandler = new DeliveryRequestHandler(_httpClient, simpleDidDocResolverForAlice, secretResolverInMemoryForAlice);
        var limit = 100;
        var deliveryRequestResult = await _deliveryRequestHandler.Handle(new DeliveryRequestRequest(localDidOfAliceToUseWithTheMediator.Value.PeerDid.Value, requestMediationResult.Value.MediatorDid, requestMediationResult.Value.MediatorEndpoint, limit), new CancellationToken());

        // Assert
        deliveryRequestResult.IsSuccess.Should().BeTrue();
        deliveryRequestResult.Value.Status.Should().NotBeNull();
        deliveryRequestResult.Value.Status.MessageCount.Should().Be(0);
    }

    /// <summary>
    /// This tests assumes that a PRISM Mediator is running on https://beta-mediator.atalaprism.io
    /// </summary>
    /// /// NOT PASSING THIS TEST AS OF October 4, 2023
    [Fact]
    public async Task AliceWantsToPickUpMessagesButAnUnregisteredDidCausesAProblemReport()
    {
        // First get the OOB from the running mediator
        var response = await _httpClient.GetAsync(_prismMediatorUri + "invitationOOB");
        var resultContent = await response.Content.ReadAsStringAsync();
        var oob = resultContent.Split("=");
        var oobInvitation = oob[1];

        var secretResolverInMemoryForAlice = new SecretResolverInMemory();
        var simpleDidDocResolverForAlice = new SimpleDidDocResolver();
        _createPeerDidHandlerAlice = new CreatePeerDidHandler(secretResolverInMemoryForAlice);

        var localDidOfAliceToUseWithTheMediator = await _createPeerDidHandlerAlice.Handle(new CreatePeerDidRequest(), cancellationToken: new CancellationToken());
        var request = new RequestMediationRequest(oobInvitation, localDidOfAliceToUseWithTheMediator.Value.PeerDid.Value);

        _requestMediationHandler = new RequestMediationHandler(_httpClient, simpleDidDocResolverForAlice, secretResolverInMemoryForAlice);
        var requestMediationResult = await _requestMediationHandler.Handle(request, CancellationToken.None);

        var someUnregisteredDid = await _createPeerDidHandlerAlice.Handle(new CreatePeerDidRequest(), cancellationToken: new CancellationToken());

        _deliveryRequestHandler = new DeliveryRequestHandler(_httpClient, simpleDidDocResolverForAlice, secretResolverInMemoryForAlice);
        var limit = 100;
        var deliveryRequestResult = await _deliveryRequestHandler.Handle(new DeliveryRequestRequest(someUnregisteredDid.Value.PeerDid.Value, requestMediationResult.Value.MediatorDid, requestMediationResult.Value.MediatorEndpoint, limit), new CancellationToken());

        // Assert
        deliveryRequestResult.IsSuccess.Should().BeTrue();
        deliveryRequestResult.Value.ProblemReport.Should().NotBeNull();
    }

    /// <summary>
    /// This tests assumes that a PRISM Mediator is running on https://beta-mediator.atalaprism.io
    /// </summary>
    /// /// NOT PASSING THIS TEST AS OF October 4, 2023
    [Fact]
    public async Task AliceWantsToGetMessageStatusForUnregisteredRecipeintDidCausesProblemReport()
    {
        // First get the OOB from the running mediator
        var response = await _httpClient.GetAsync(_prismMediatorUri + "invitationOOB");
        var resultContent = await response.Content.ReadAsStringAsync();
        var oob = resultContent.Split("=");
        var oobInvitation = oob[1];

        var secretResolverInMemoryForAlice = new SecretResolverInMemory();
        var simpleDidDocResolverForAlice = new SimpleDidDocResolver();
        _createPeerDidHandlerAlice = new CreatePeerDidHandler(secretResolverInMemoryForAlice);

        var localDidOfAliceToUseWithTheMediator = await _createPeerDidHandlerAlice.Handle(new CreatePeerDidRequest(), cancellationToken: new CancellationToken());
        var request = new RequestMediationRequest(oobInvitation, localDidOfAliceToUseWithTheMediator.Value.PeerDid.Value);

        _requestMediationHandler = new RequestMediationHandler(_httpClient, simpleDidDocResolverForAlice, secretResolverInMemoryForAlice);
        var requestMediationResult = await _requestMediationHandler.Handle(request, CancellationToken.None);

        var unregisteredDid = await _createPeerDidHandlerAlice.Handle(new CreatePeerDidRequest(), cancellationToken: new CancellationToken());

        // Alice asks the Mediator for new Messages 
        _statusRequestHandler = new StatusRequestHandler(_httpClient, simpleDidDocResolverForAlice, secretResolverInMemoryForAlice);
        var statusRequestResult = await _statusRequestHandler.Handle(new StatusRequestRequest(localDidOfAliceToUseWithTheMediator.Value.PeerDid.Value, requestMediationResult.Value.MediatorDid, requestMediationResult.Value.MediatorEndpoint, unregisteredDid.Value.PeerDid.Value),
            new CancellationToken());

        // Assert
        statusRequestResult.IsSuccess.Should().BeTrue();
        statusRequestResult.Value.ProblemReport.Should().NotBeNull();
    }

    /// <summary>
    /// This tests assumes that a PRISM Mediator is running on https://beta-mediator.atalaprism.io
    /// </summary>
    /// /// NOT PASSING THIS TEST AS OF October 4, 2023
    [Fact]
    public async Task AliceWantsToGetMessageStatusForUnregisteredSenderDidCausesProblemReport()
    {
        // First get the OOB from the running mediator
        var response = await _httpClient.GetAsync(_prismMediatorUri + "invitationOOB");
        var resultContent = await response.Content.ReadAsStringAsync();
        var oob = resultContent.Split("=");
        var oobInvitation = oob[1];

        var secretResolverInMemoryForAlice = new SecretResolverInMemory();
        var simpleDidDocResolverForAlice = new SimpleDidDocResolver();
        _createPeerDidHandlerAlice = new CreatePeerDidHandler(secretResolverInMemoryForAlice);

        var localDidOfAliceToUseWithTheMediator = await _createPeerDidHandlerAlice.Handle(new CreatePeerDidRequest(), cancellationToken: new CancellationToken());
        var request = new RequestMediationRequest(oobInvitation, localDidOfAliceToUseWithTheMediator.Value.PeerDid.Value);

        _requestMediationHandler = new RequestMediationHandler(_httpClient, simpleDidDocResolverForAlice, secretResolverInMemoryForAlice);
        var requestMediationResult = await _requestMediationHandler.Handle(request, CancellationToken.None);

        var unregisteredDid = await _createPeerDidHandlerAlice.Handle(new CreatePeerDidRequest(), cancellationToken: new CancellationToken());

        // Alice asks the Mediator for new Messages 
        _statusRequestHandler = new StatusRequestHandler(_httpClient, simpleDidDocResolverForAlice, secretResolverInMemoryForAlice);
        var statusRequestResult = await _statusRequestHandler.Handle(new StatusRequestRequest(unregisteredDid.Value.PeerDid.Value, requestMediationResult.Value.MediatorDid, requestMediationResult.Value.MediatorEndpoint),
            new CancellationToken());

        // Assert
        statusRequestResult.IsSuccess.Should().BeTrue();
        statusRequestResult.Value.ProblemReport.Should().NotBeNull();
    }

    /// <summary>
    /// This tests assumes that a PRISM Mediator is running on https://beta-mediator.atalaprism.io
    /// </summary>
    /// NOT PASSING THIS TEST AS OF October 4, 2023
    [Fact]
    public async Task BobSendsBasicMessageToAliceAndForwardmessageFailsWithProblemReport()
    {
        // First get the OOB from the running mediator
        var response = await _httpClient.GetAsync(_prismMediatorUri + "invitationOOB");
        var resultContent = await response.Content.ReadAsStringAsync();
        var oob = resultContent.Split("=");
        var oobInvitation = oob[1];

        var secretResolverInMemoryForAlice = new SecretResolverInMemory();
        var simpleDidDocResolverForAlice = new SimpleDidDocResolver();
        _createPeerDidHandlerAlice = new CreatePeerDidHandler(secretResolverInMemoryForAlice);

        var localDidOfAliceToUseWithTheMediator = await _createPeerDidHandlerAlice.Handle(new CreatePeerDidRequest(), cancellationToken: new CancellationToken());
        var request = new RequestMediationRequest(oobInvitation, localDidOfAliceToUseWithTheMediator.Value.PeerDid.Value);

        _requestMediationHandler = new RequestMediationHandler(_httpClient, simpleDidDocResolverForAlice, secretResolverInMemoryForAlice);
        var requestMediationResult = await _requestMediationHandler.Handle(request, CancellationToken.None);

        // Alice create now an additional DID to be used with Bob. Important: The service endpoint of the DID must be set to the mediator endpoint
        var localDidOfAliceToUseWithBob = await _createPeerDidHandlerAlice.Handle(new CreatePeerDidRequest(requestMediationResult.Value.RoutingDid), cancellationToken: new CancellationToken());

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
        var basicMessage = BasicMessage.Create("Hello Alice", localDidOfBobToUseWithAlice.Value.PeerDid.Value);
        var packedBasicMessage = await BasicMessage.Pack(basicMessage, from: localDidOfBobToUseWithAlice.Value.PeerDid.Value, localDidOfAliceToUseWithBob.Value.PeerDid.Value, secretResolverInMemoryForBob, simpleDidDocResolverForBob);

        // Bob creates a DID just to be used with the mediator
        var localDidOfBobToUseWithAliceMediator = await _createPeerDidHandlerBob.Handle(new CreatePeerDidRequest(), cancellationToken: new CancellationToken());

        // Wrap the Basic Message into a new Message for the mediator to recieve and send it

        var invalidRecipientDid = await _createPeerDidHandlerAlice.Handle(new CreatePeerDidRequest(), cancellationToken: new CancellationToken());
        var resolvedMediatorDid = PeerDidResolver.ResolvePeerDid(new PeerDid(localDidOfAliceToUseWithBob.Value.DidDoc.Services.First().ServiceEndpoint.Uri), VerificationMaterialFormatPeerDid.Jwk);
        var resolvedMediatorDidDoc = DidDocPeerDid.FromJson(resolvedMediatorDid.Value);
        var resolvedMediatorDidEndpoint = resolvedMediatorDidDoc.Value.Services.First().ServiceEndpoint;
        _sendForwardMessageHandler = new SendForwardMessageHandler(_httpClient, simpleDidDocResolverForBob, secretResolverInMemoryForBob);
        var result = await _sendForwardMessageHandler.Handle(new SendForwardMessageRequest(
            message: packedBasicMessage.Value,
            localDid: localDidOfBobToUseWithAliceMediator.Value.PeerDid.Value,
            mediatorDid: localDidOfAliceToUseWithBob.Value.DidDoc.Services.First().ServiceEndpoint.Uri, // The mediator DID was also shared beforehand (should be in the shared DID of alice)
            mediatorEndpoint: new Uri(resolvedMediatorDidEndpoint.Uri),
            recipientDid: invalidRecipientDid.Value.PeerDid.Value
        ), new CancellationToken());

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
    }
}