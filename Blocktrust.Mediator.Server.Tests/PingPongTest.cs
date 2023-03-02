namespace Blocktrust.Mediator.Server.Tests;

using Blocktrust.Common.Resolver;
using Common;
using Server.Controllers;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Resolver;

// using FluentResults;

public class PingPongTest
{
    private readonly Mock<ILogger<MediatorController>> _iLogger;
    private readonly Mock<IMediator> _mediatorMock;
    private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
    private readonly ISecretResolver _secretResolver;
    private readonly IDidDocResolver _didDocResolver;

    public PingPongTest()
    {
        _iLogger = new Mock<ILogger<MediatorController>>();
        _mediatorMock = new Mock<IMediator>();
        _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        _secretResolver = new MediatorSecretResolver(_mediatorMock.Object);
        _didDocResolver = new SimpleDidDocResolver();
    }

    [Fact]
    public async Task PingPongWorksAsExpected()
    {
        // Arrange
        var controller = new MediatorController(_iLogger.Object, _mediatorMock.Object, _httpContextAccessorMock.Object, _secretResolver, _didDocResolver);

        // Act
        var result = await controller.Ping("123");

        // Assert
        result.Result.Should().BeOfType(typeof(OkObjectResult));
        var resultContent = (OkObjectResult)result.Result!;
        resultContent.Value.Should().Be("Pong 123");

    }
}