namespace Blocktrust.Mediator.Client.RootsIntegrationTests;

using System.Text;
using System.Text.Json;
using Blocktrust.Common.Converter;
using Blocktrust.Mediator.Common.Commands.CreatePeerDid;
using Commands.DiscoverFeatures;
using Commands.MediatorCoordinator.QueryKeys;
using Commands.ShortenUrl;
using Common;
using Common.Models.DiscoverFeatures;
using Common.Models.OutOfBand;
using Common.Models.ShortenUrl;
using DIDComm.Secrets;
using FluentAssertions;
using PeerDID.DIDDoc;
using PeerDID.PeerDIDCreateResolve;
using PeerDID.Types;
using Xunit;

public class ShortenUrlTests
{
    private readonly HttpClient _httpClient;
    private RequestShortenedUrlHandler _requestShortenedUrlHandler;
    private InvalidateShortenedUrlHandler _invalidateShortenedUrlHandler;
    private CreatePeerDidHandler _createPeerDidHandler;

    public ShortenUrlTests()
    {
        _httpClient = new HttpClient();
    }

    /// <summary>
    /// This tests assumes that the Roots Mediator is running on http://127.0.0.1:8000
    /// </summary>
    [Fact]
    public async Task GenerateShortenedUrlSucceeds()
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
        var urlToShorten = new Uri(
            "http://127.0.0.1:8000?_oob=eyJ0eXBlIjoiaHR0cHM6Ly9kaWRjb21tLm9yZy9vdXQtb2YtYmFuZC8yLjAvaW52aXRhdGlvbiIsImlkIjoiMzFlMWY3ODQtNmUwZC00NDI3LTk1OTYtNDQwNDkzOTI3YWJlIiwiZnJvbSI6ImRpZDpwZWVyOjIuRXo2TFNkRWNzVnZ6ZTNjWkpxaTFLRFdyU2N5MmFINW9IOFlkUVJRaTZmNVpIN1lGMi5WejZNa284aXllUDRITTFpS3ZuY2o3TkVRQ3JjeXpQN1Y1VW1ad2N1QXN6NWhZRkJlLlNleUpwWkNJNkltNWxkeTFwWkNJc0luUWlPaUprYlNJc0luTWlPaUpvZEhSd09pOHZNVEkzTGpBdU1DNHhPamd3TURBaUxDSmhJanBiSW1ScFpHTnZiVzB2ZGpJaVhYMCIsImJvZHkiOnsiZ29hbF9jb2RlIjoicmVxdWVzdC1tZWRpYXRlIiwiZ29hbCI6IlJlcXVlc3RNZWRpYXRlIiwibGFiZWwiOiJNZWRpYXRvciIsImFjY2VwdCI6WyJkaWRjb21tL3YyIl19fQ");
        long? requestValidityInSeconds = null;
        var goalCode = EnumShortenUrlGoalCode.ShortenOOBv2;
        string? shortUrlSlug = null;
        var request = new RequestShortenedUrlRequest(new Uri(mediatorEndpoint), mediatorDid, localDid.Value.PeerDid.Value, urlToShorten, goalCode, requestValidityInSeconds, shortUrlSlug);

        // Act
        _requestShortenedUrlHandler = new RequestShortenedUrlHandler(_httpClient, new SimpleDidDocResolver(), secretResolverInMemory);
        var result = await _requestShortenedUrlHandler.Handle(request, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.ShortenedUrl.Should().NotBeNull();
        result.Value.ExpiresTimeUtc.Should().NotBeNull();
        result.Value.ShortenedUrl.AbsoluteUri.Should().Contain("/qr");
        result.Value.ShortenedUrl.AbsoluteUri.Should().Contain("?_oobid=");
    }

    // TODO test the rediection!
    /// <summary>
    /// This tests assumes that the Roots Mediator is running on http://127.0.0.1:8000
    /// </summary>
    [Fact]
    public async Task GenerateShortenedUrlAndInvalidationSucceeds()
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
        var urlToShorten = new Uri(
            "http://127.0.0.1:8000?_oob=eyJ0eXBlIjoiaHR0cHM6Ly9kaWRjb21tLm9yZy9vdXQtb2YtYmFuZC8yLjAvaW52aXRhdGlvbiIsImlkIjoiMzFlMWY3ODQtNmUwZC00NDI3LTk1OTYtNDQwNDkzOTI3YWJlIiwiZnJvbSI6ImRpZDpwZWVyOjIuRXo2TFNkRWNzVnZ6ZTNjWkpxaTFLRFdyU2N5MmFINW9IOFlkUVJRaTZmNVpIN1lGMi5WejZNa284aXllUDRITTFpS3ZuY2o3TkVRQ3JjeXpQN1Y1VW1ad2N1QXN6NWhZRkJlLlNleUpwWkNJNkltNWxkeTFwWkNJc0luUWlPaUprYlNJc0luTWlPaUpvZEhSd09pOHZNVEkzTGpBdU1DNHhPamd3TURBaUxDSmhJanBiSW1ScFpHTnZiVzB2ZGpJaVhYMCIsImJvZHkiOnsiZ29hbF9jb2RlIjoicmVxdWVzdC1tZWRpYXRlIiwiZ29hbCI6IlJlcXVlc3RNZWRpYXRlIiwibGFiZWwiOiJNZWRpYXRvciIsImFjY2VwdCI6WyJkaWRjb21tL3YyIl19fQ");
        long? requestValidityInSeconds = null;
        var goalCode = EnumShortenUrlGoalCode.ShortenOOBv2;
        string? shortUrlSlug = null;
        var request = new RequestShortenedUrlRequest(new Uri(mediatorEndpoint), mediatorDid, localDid.Value.PeerDid.Value, urlToShorten, goalCode, requestValidityInSeconds, shortUrlSlug);

        _requestShortenedUrlHandler = new RequestShortenedUrlHandler(_httpClient, new SimpleDidDocResolver(), secretResolverInMemory);
        var resultShortenUrl = await _requestShortenedUrlHandler.Handle(request, CancellationToken.None);

        resultShortenUrl.IsSuccess.Should().BeTrue();
        
        _invalidateShortenedUrlHandler = new InvalidateShortenedUrlHandler(_httpClient, new SimpleDidDocResolver(), secretResolverInMemory);
        var result = await _invalidateShortenedUrlHandler.Handle(new InvalidateShortenedUrlRequest(new Uri(mediatorEndpoint), mediatorDid, localDid.Value.PeerDid.Value, resultShortenUrl.Value.ShortenedUrl), new CancellationToken());
        
        result.IsSuccess.Should().BeTrue();
    }
    
    //TODO test path
    
    //TODO test validity
}