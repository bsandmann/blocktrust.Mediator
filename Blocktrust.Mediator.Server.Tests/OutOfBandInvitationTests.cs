namespace Blocktrust.Mediator.Server.Tests;

using Blocktrust.Common.Resolver;
using Commands.DatabaseCommands.CreateOobInvitation;
using Commands.DatabaseCommands.GetOobInvitation;
using Common;
using Common.Commands.CreatePeerDid;
using Common.Models.OutOfBand;
using Controllers;
using DIDComm.Secrets;
using Entities;
using FluentAssertions;
using FluentResults;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Models;
using Moq;
using Resolver;

public class OutOfBandInvitationTests
{
    private readonly Mock<ILogger<OobController>> _iLogger;
    private readonly Mock<IMediator> _mediatorMock;
    private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
    private readonly CreatePeerDidHandler _createPeerDidHandler;
    private readonly ISecretResolver _secretResolver;
    private readonly IDidDocResolver _didDocResolver;

    public OutOfBandInvitationTests()
    {
        _iLogger = new Mock<ILogger<OobController>>();
        _mediatorMock = new Mock<IMediator>();
        _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        _secretResolver = new SecretResolverInMemory();
        _didDocResolver = new SimpleDidDocResolver();
        _createPeerDidHandler = new CreatePeerDidHandler(_secretResolver);
        
        _httpContextAccessorMock.Setup(p => p.HttpContext.Request.Host).Returns(new HostString("dummydomain.com"));
        _httpContextAccessorMock.Setup(p => p.HttpContext.Request.Scheme).Returns("https");
        
        
    }

    [Fact]
    public async Task OutOfBandInvitationGetsLoadedFromDatabase()
    {
        // Arrange
        _mediatorMock.Setup(p => p.Send(It.IsAny<GetOobInvitationRequest>(), It.IsAny<CancellationToken>()))
            .Returns(async (GetOobInvitationRequest request, CancellationToken token) => Result.Ok(new OobInvitationModel(new OobInvitationEntity()
        {
            OobId = Guid.NewGuid(),
            CreatedUtc = DateTime.UtcNow,
            Did="TheDIDofTheMediator",
            Invitation = "TheInvitationIntheDatabase",
            Url = "TheUrlTheMediatorIsListeningOn",
            
        })));
        var controller = new OobController(_iLogger.Object, _mediatorMock.Object, _httpContextAccessorMock.Object, _secretResolver, _didDocResolver);
        
        // Act
        var result = await controller.OutOfBandInvitation();
       
        // Assert
        result.Result.Should().BeOfType(typeof(OkObjectResult));
        var resultContent = (OkObjectResult)result.Result;
        resultContent.Value.Should().Be("https://dummydomain.com?_oob=TheInvitationIntheDatabase");
    }
    
    [Fact]
    public async Task OutOfBandInvitationGetsCreated()
    {
        // Arrange
        _mediatorMock.Setup(p => p.Send(It.IsAny<GetOobInvitationRequest>(), It.IsAny<CancellationToken>()))
            .Returns(async (GetOobInvitationRequest request, CancellationToken token) => Result.Fail("Not in database"));
        _mediatorMock.Setup(p => p.Send(It.IsAny<CreatePeerDidRequest>(), It.IsAny<CancellationToken>()))
            .Returns(async (CreatePeerDidRequest request, CancellationToken token) => await _createPeerDidHandler.Handle(request, token) );
        _mediatorMock.Setup(p => p.Send(It.IsAny<CreateOobInvitationRequest>(), It.IsAny<CancellationToken>()))
            .Returns(async (CreateOobInvitationRequest request, CancellationToken token) => Result.Ok(new OobInvitationModel(new OobInvitationEntity()
                {
                    Did = request.PeerDid.Value,
                    CreatedUtc = DateTime.UtcNow,
                    Url = request.HostUrl,
                    Invitation = OobModel.BuildRequestMediateMessage(request.PeerDid)
                }
                )) );
        var controller = new OobController(_iLogger.Object, _mediatorMock.Object, _httpContextAccessorMock.Object,  _secretResolver, _didDocResolver);
        
        // Act
        var result = await controller.OutOfBandInvitation();
       
        // Assert
        result.Result.Should().BeOfType(typeof(OkObjectResult));
        var resultContent = (OkObjectResult)result.Result;
        var resultContentString = (string)resultContent.Value;
        resultContentString.Should().StartWith("https://dummydomain.com?_oob=eyJ0eX");
    }
}