﻿namespace Blocktrust.Mediator.Server.Tests;

using Blocktrust.Common.Resolver;
using Common;
using Controllers;
using DIDComm;
using DIDComm.Message.Messages;
using DIDComm.Model.PackEncryptedParamsModels;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Protocols.TrustPing.Models;
using Resolver;
using TestData.DIDDoc;
using TestData.Secrets;

// using FluentResults;

public class TrustPingTests
{
    private readonly Mock<ILogger<MediatorController>> _iLogger;
    private readonly Mock<IMediator> _mediatorMock;
    private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
    private const string ALICE_DID = "did:example:alice";
    private const string BOB_DID = "did:example:bob";
    private const string RootsMediatorLocal = "did:peer:2.Ez6LSk8oEwmAfG1JyV4oG9JrUuswJobRhx4RkVsc7uaAYirYK.Vz6Mkgm5gQ13JisT9HPh7oQUsTeAHMWZoQzzsYD5oP2Y9rqCs#6LSk8oEwmAfG1JyV4oG9JrUuswJobRhx4RkVsc7uaAYirYK";
    private readonly ISecretResolver _secretResolver;
    private readonly IDidDocResolver _didDocResolver;
    

    public TrustPingTests()
    {
        _iLogger = new Mock<ILogger<MediatorController>>();
        _mediatorMock = new Mock<IMediator>();
        _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        _secretResolver = new MediatorSecretResolver(_mediatorMock.Object);
        _didDocResolver = new SimpleDidDocResolver();
    }

    [Fact]
    public async Task TrustPingWorksAsExpected()
    {
        // Arrange
        var controller = new MediatorController(_iLogger.Object, _mediatorMock.Object, _httpContextAccessorMock.Object, _secretResolver, _didDocResolver);


        var trustPingMessage = new TrustPingRequest(from: "abc", true);
        var json = trustPingMessage.Serialize();

        var didComm = new DidComm(new DidDocResolverMock(), new RootsMediatorSecretResolverMock());
        var message = Message.Builder(
                id: "1234567890",
                body: new Dictionary<string, object> { { "response_requested", true } },
                type: "https://didcomm.org/trust-ping/2.0/ping"
            )
            .from(ALICE_DID)
            .to(new List<string> { RootsMediatorLocal })
            .createdTime(1516269022)
            .expiresTime(1516385931)
            .build();
        var param = new PackEncryptedParamsBuilder(message,RootsMediatorLocal).BuildPackEncryptedParams();
        var packResult = didComm.PackEncrypted(param);


        // Act
        var result = await controller.Mediate();

        // Assert
        result.Result.Should().BeOfType(typeof(OkObjectResult));
        var resultContent = (OkObjectResult)result.Result!;
        resultContent.Value.Should().Be("Pong 123");
    }
}