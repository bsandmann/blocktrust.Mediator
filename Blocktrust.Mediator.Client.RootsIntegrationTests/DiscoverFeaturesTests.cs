namespace Blocktrust.Mediator.Client.RootsIntegrationTests;

using System.Text;
using System.Text.Json;
using Blocktrust.Common.Converter;
using Blocktrust.Mediator.Common.Commands.CreatePeerDid;
using Commands.DiscoverFeatures;
using Commands.MediatorCoordinator.QueryKeys;
using Common;
using Common.Models.DiscoverFeatures;
using Common.Models.OutOfBand;
using DIDComm.Secrets;
using FluentAssertions;
using PeerDID.DIDDoc;
using PeerDID.PeerDIDCreateResolve;
using PeerDID.Types;
using Xunit;

public class DiscoverFeaturesTest
{
    private readonly HttpClient _httpClient;
    private DiscoverFeaturesHandler _discoverFeaturesHandler;
    private CreatePeerDidHandler _createPeerDidHandler;

    public DiscoverFeaturesTest()
    {
        _httpClient = new HttpClient();
    }

    /// <summary>
    /// This tests assumes that the Roots Mediator is running on http://127.0.0.1:8000
    /// </summary>
    [Fact]
    public async Task DiscoverFeatuesWithSimpleQueryMatchingEverything()
    {
        // Arrange
        var oobInvitationRootsLocal =
            "eyJ0eXBlIjoiaHR0cHM6Ly9kaWRjb21tLm9yZy9vdXQtb2YtYmFuZC8yLjAvaW52aXRhdGlvbiIsImlkIjoiNGZjN2Q3NDYtMzk2Ny00NjFjLTg5MTAtMWM5YTBmMjdkYjQ0IiwiZnJvbSI6ImRpZDpwZWVyOjIuRXo2TFNkRWNzVnZ6ZTNjWkpxaTFLRFdyU2N5MmFINW9IOFlkUVJRaTZmNVpIN1lGMi5WejZNa284aXllUDRITTFpS3ZuY2o3TkVRQ3JjeXpQN1Y1VW1ad2N1QXN6NWhZRkJlLlNleUpwWkNJNkltNWxkeTFwWkNJc0luUWlPaUprYlNJc0luTWlPaUpvZEhSd09pOHZNVEkzTGpBdU1DNHhPamd3TURBaUxDSmhJanBiSW1ScFpHTnZiVzB2ZGpJaVhYMCIsImJvZHkiOnsiZ29hbF9jb2RlIjoicmVxdWVzdC1tZWRpYXRlIiwiZ29hbCI6IlJlcXVlc3RNZWRpYXRlIiwibGFiZWwiOiJNZWRpYXRvciIsImFjY2VwdCI6WyJkaWRjb21tL3YyIl19fQ";

        var decodedInvitation = Encoding.UTF8.GetString(Base64Url.Decode(oobInvitationRootsLocal));
        var oobModel = JsonSerializer.Deserialize<OobModel>(decodedInvitation);
        var invitationPeerDidResult = PeerDidResolver.ResolvePeerDid(new PeerDid(oobModel.From), VerificationMaterialFormatPeerDid.Jwk);
        var invitationPeerDidDocResult = DidDocPeerDid.FromJson(invitationPeerDidResult.Value);

        var mediatorDid = invitationPeerDidDocResult.Value.Did;
        var mediatorEndpoint = invitationPeerDidDocResult.Value.Services.FirstOrDefault().ServiceEndpoint;

        var secretResolverInMemory = new SecretResolverInMemory();
        _createPeerDidHandler = new CreatePeerDidHandler(secretResolverInMemory);

        var localDid = await _createPeerDidHandler.Handle(new CreatePeerDidRequest(), cancellationToken: new CancellationToken());
        var queries = new List<FeatureQuery>();
        queries.Add(new FeatureQuery("protocol"));
        var request = new DiscoverFeaturesRequest(new Uri(mediatorEndpoint),mediatorDid, localDid.Value.PeerDid.Value, queries);

        // Act
        _discoverFeaturesHandler = new DiscoverFeaturesHandler(_httpClient, new SimpleDidDocResolver(), secretResolverInMemory);
        var result = await _discoverFeaturesHandler.Handle(request, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Count.Should().Be(8);
        result.Value[0].FeatureType.Should().Be("protocol");
        result.Value[0].Id.Should().Be("https://didcomm.org/basicmessage/2.0");
    }
    
    /// <summary>
    /// This tests assumes that the Roots Mediator is running on http://127.0.0.1:8000
    /// </summary>
    [Fact]
    public async Task DiscoverFeatuesWithSimpleQueryMatchingOnlyCoordinateMediation()
    {
        // Arrange
        var oobInvitationRootsLocal =
            "eyJ0eXBlIjoiaHR0cHM6Ly9kaWRjb21tLm9yZy9vdXQtb2YtYmFuZC8yLjAvaW52aXRhdGlvbiIsImlkIjoiNGZjN2Q3NDYtMzk2Ny00NjFjLTg5MTAtMWM5YTBmMjdkYjQ0IiwiZnJvbSI6ImRpZDpwZWVyOjIuRXo2TFNkRWNzVnZ6ZTNjWkpxaTFLRFdyU2N5MmFINW9IOFlkUVJRaTZmNVpIN1lGMi5WejZNa284aXllUDRITTFpS3ZuY2o3TkVRQ3JjeXpQN1Y1VW1ad2N1QXN6NWhZRkJlLlNleUpwWkNJNkltNWxkeTFwWkNJc0luUWlPaUprYlNJc0luTWlPaUpvZEhSd09pOHZNVEkzTGpBdU1DNHhPamd3TURBaUxDSmhJanBiSW1ScFpHTnZiVzB2ZGpJaVhYMCIsImJvZHkiOnsiZ29hbF9jb2RlIjoicmVxdWVzdC1tZWRpYXRlIiwiZ29hbCI6IlJlcXVlc3RNZWRpYXRlIiwibGFiZWwiOiJNZWRpYXRvciIsImFjY2VwdCI6WyJkaWRjb21tL3YyIl19fQ";

        var decodedInvitation = Encoding.UTF8.GetString(Base64Url.Decode(oobInvitationRootsLocal));
        var oobModel = JsonSerializer.Deserialize<OobModel>(decodedInvitation);
        var invitationPeerDidResult = PeerDidResolver.ResolvePeerDid(new PeerDid(oobModel.From), VerificationMaterialFormatPeerDid.Jwk);
        var invitationPeerDidDocResult = DidDocPeerDid.FromJson(invitationPeerDidResult.Value);

        var mediatorDid = invitationPeerDidDocResult.Value.Did;
        var mediatorEndpoint = invitationPeerDidDocResult.Value.Services.FirstOrDefault().ServiceEndpoint;

        var secretResolverInMemory = new SecretResolverInMemory();
        _createPeerDidHandler = new CreatePeerDidHandler(secretResolverInMemory);

        var localDid = await _createPeerDidHandler.Handle(new CreatePeerDidRequest(), cancellationToken: new CancellationToken());
        var queries = new List<FeatureQuery>();
        queries.Add(new FeatureQuery("protocol","https://didcomm.org/coordinate-mediation/2.0"));
        var request = new DiscoverFeaturesRequest(new Uri(mediatorEndpoint),mediatorDid, localDid.Value.PeerDid.Value, queries);

        // Act
        _discoverFeaturesHandler = new DiscoverFeaturesHandler(_httpClient, new SimpleDidDocResolver(), secretResolverInMemory);
        var result = await _discoverFeaturesHandler.Handle(request, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Count.Should().Be(1);
        result.Value[0].FeatureType.Should().Be("protocol");
        result.Value[0].Id.Should().Be("https://didcomm.org/coordinate-mediation/2.0");
    }
    
     /// <summary>
    /// This tests assumes that the Roots Mediator is running on http://127.0.0.1:8000
    /// </summary>
    [Fact]
    public async Task DiscoverFeatuesWithSimpleQueryMatchingOnlyCoordinateMediationStar()
    {
        // Arrange
        var oobInvitationRootsLocal =
            "eyJ0eXBlIjoiaHR0cHM6Ly9kaWRjb21tLm9yZy9vdXQtb2YtYmFuZC8yLjAvaW52aXRhdGlvbiIsImlkIjoiNGZjN2Q3NDYtMzk2Ny00NjFjLTg5MTAtMWM5YTBmMjdkYjQ0IiwiZnJvbSI6ImRpZDpwZWVyOjIuRXo2TFNkRWNzVnZ6ZTNjWkpxaTFLRFdyU2N5MmFINW9IOFlkUVJRaTZmNVpIN1lGMi5WejZNa284aXllUDRITTFpS3ZuY2o3TkVRQ3JjeXpQN1Y1VW1ad2N1QXN6NWhZRkJlLlNleUpwWkNJNkltNWxkeTFwWkNJc0luUWlPaUprYlNJc0luTWlPaUpvZEhSd09pOHZNVEkzTGpBdU1DNHhPamd3TURBaUxDSmhJanBiSW1ScFpHTnZiVzB2ZGpJaVhYMCIsImJvZHkiOnsiZ29hbF9jb2RlIjoicmVxdWVzdC1tZWRpYXRlIiwiZ29hbCI6IlJlcXVlc3RNZWRpYXRlIiwibGFiZWwiOiJNZWRpYXRvciIsImFjY2VwdCI6WyJkaWRjb21tL3YyIl19fQ";

        var decodedInvitation = Encoding.UTF8.GetString(Base64Url.Decode(oobInvitationRootsLocal));
        var oobModel = JsonSerializer.Deserialize<OobModel>(decodedInvitation);
        var invitationPeerDidResult = PeerDidResolver.ResolvePeerDid(new PeerDid(oobModel.From), VerificationMaterialFormatPeerDid.Jwk);
        var invitationPeerDidDocResult = DidDocPeerDid.FromJson(invitationPeerDidResult.Value);

        var mediatorDid = invitationPeerDidDocResult.Value.Did;
        var mediatorEndpoint = invitationPeerDidDocResult.Value.Services.FirstOrDefault().ServiceEndpoint;

        var secretResolverInMemory = new SecretResolverInMemory();
        _createPeerDidHandler = new CreatePeerDidHandler(secretResolverInMemory);

        var localDid = await _createPeerDidHandler.Handle(new CreatePeerDidRequest(), cancellationToken: new CancellationToken());
        var queries = new List<FeatureQuery>();
        queries.Add(new FeatureQuery("protocol","https://didcomm.org/coordinate-mediation.*"));
        var request = new DiscoverFeaturesRequest(new Uri(mediatorEndpoint),mediatorDid, localDid.Value.PeerDid.Value, queries);

        // Act
        _discoverFeaturesHandler = new DiscoverFeaturesHandler(_httpClient, new SimpleDidDocResolver(), secretResolverInMemory);
        var result = await _discoverFeaturesHandler.Handle(request, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Count.Should().Be(1);
        result.Value[0].FeatureType.Should().Be("protocol");
        result.Value[0].Id.Should().Be("https://didcomm.org/coordinate-mediation/2.0");
    }
}