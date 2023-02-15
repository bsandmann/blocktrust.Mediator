namespace Blocktrust.Mediator.Tests;

using Blocktrust.Mediator.Controllers;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

// using FluentResults;

public class PingPongTest
{
    private readonly Mock<ILogger<MediatorController>> _iLogger;
    private readonly Mock<IMediator> _mediatorMock;

    public PingPongTest()
    {
        _iLogger = new Mock<ILogger<MediatorController>>();
        _mediatorMock = new Mock<IMediator>();
    }

    [Fact]
    public async Task PingPongWorksAsExpected()
    {
        // Arrange
        var controller = new MediatorController(_iLogger.Object, _mediatorMock.Object);

        // Act
        var result = await controller.Ping("123");

        // Assert
        result.Result.Should().BeOfType(typeof(OkObjectResult));
        var resultContent = (OkObjectResult)result.Result!;
        resultContent.Value.Should().Be("Pong 123");

    }
}