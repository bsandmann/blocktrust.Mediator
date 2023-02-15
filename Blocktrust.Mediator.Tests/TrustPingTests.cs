namespace Blocktrust.Mediator.Tests;

using Blocktrust.Mediator.Controllers;
using Blocktrust.Mediator.Protocols.TrustPing.Models;
using DIDComm_v2;
using DIDComm_v2.Message.Messages;
using DIDComm_v2.Model.PackEncryptedParamsModels;
using DIDComm_v2.Model.PackPlaintextParamsModels;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using TestData.DIDDoc;
using TestData.Secrets;

// using FluentResults;

public class TrustPingTests
{
    private readonly Mock<ILogger<MediatorController>> _iLogger;
    private readonly Mock<IMediator> _mediatorMock;
    private const string ALICE_DID = "did:example:alice";
    private const string BOB_DID = "did:example:bob";
    private const string RootsMediatorLocal = "did:peer:2.Ez6LSk8oEwmAfG1JyV4oG9JrUuswJobRhx4RkVsc7uaAYirYK.Vz6Mkgm5gQ13JisT9HPh7oQUsTeAHMWZoQzzsYD5oP2Y9rqCs#6LSk8oEwmAfG1JyV4oG9JrUuswJobRhx4RkVsc7uaAYirYK";
    

    public TrustPingTests()
    {
        _iLogger = new Mock<ILogger<MediatorController>>();
        _mediatorMock = new Mock<IMediator>();
    }

    [Fact]
    public async Task TrustPingWorksAsExpected()
    {
        // Arrange
        var controller = new MediatorController(_iLogger.Object, _mediatorMock.Object);


        var trustPingMessage = new TrustPingRequest(from: "abc", true);
        var json = trustPingMessage.Serialize();

        var didComm = new DIDComm(new DIDDocResolverMock(), new RootsMediatorSecretResolverMock());
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