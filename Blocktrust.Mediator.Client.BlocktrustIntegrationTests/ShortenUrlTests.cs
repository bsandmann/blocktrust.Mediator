namespace Blocktrust.Mediator.Client.BlocktrustIntegrationTests;

using System.Text;
using System.Text.Json;
using Blocktrust.Common.Converter;
using Blocktrust.DIDComm.Secrets;
using Blocktrust.Mediator.Client.Commands.ShortenUrl;
using Blocktrust.Mediator.Common;
using Blocktrust.Mediator.Common.Commands.CreatePeerDid;
using Blocktrust.Mediator.Common.Models.DiscoverFeatures;
using Blocktrust.Mediator.Common.Models.OutOfBand;
using Blocktrust.PeerDID.DIDDoc;
using Blocktrust.PeerDID.PeerDIDCreateResolve;
using Blocktrust.PeerDID.Types;
using Commands.ShortenUrl.InvalidateShortenedUrl;
using Commands.ShortenUrl.RequestShortenedUrl;
using Common.Models.ShortenUrl;
using FluentAssertions;
using Xunit;

public class ShortenUrlTests
{
    private readonly HttpClient _httpClient;
    private RequestShortenedUrlHandler _requestShortenedUrlHandler;
    private CreatePeerDidHandler _createPeerDidHandler;
    private InvalidateShortenedUrlHandler _invalidateShortenedUrlHandler;
    private readonly string _blocktrustMediatorUri = "https://localhost:7037/";

    public ShortenUrlTests()
    {
        _httpClient = new HttpClient();
    }

    /// <summary>
    /// This tests assumes that the Blocktrust Mediator is running on https://localhost:7037/
    /// </summary>>
    [Fact]
    public async Task GenerateShortenedUrlSucceeds()
    {
        // Arrange
        var response = await _httpClient.GetAsync(_blocktrustMediatorUri + "oob_url");
        var resultContent = await response.Content.ReadAsStringAsync();
        var oob = resultContent.Split("=");
        var oobInvitation = oob[1];

        var decodedInvitation = Encoding.UTF8.GetString(Base64Url.Decode(oobInvitation));
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
        var urlToShorten = new Uri(resultContent);
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
        result.Value.ShortenedUrl.AbsoluteUri.Should().Contain("/qr");
        result.Value.ShortenedUrl.AbsoluteUri.Should().Contain("?_oobid=");

        // TODO test the rediection!
    }
    
        // TODO test the rediection!
    /// <summary>
    /// This tests assumes that the Roots Mediator is running on http://127.0.0.1:8000
    /// </summary>
    [Fact]
    public async Task GenerateShortenedUrlAndInvalidationSucceeds()
    {
        // Arrange
        var response = await _httpClient.GetAsync(_blocktrustMediatorUri + "oob_url");
        var resultContent = await response.Content.ReadAsStringAsync();
        var oob = resultContent.Split("=");
        var oobInvitation = oob[1];

        var decodedInvitation = Encoding.UTF8.GetString(Base64Url.Decode(oobInvitation));
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
        
        var urlToShorten = new Uri(resultContent);
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
}