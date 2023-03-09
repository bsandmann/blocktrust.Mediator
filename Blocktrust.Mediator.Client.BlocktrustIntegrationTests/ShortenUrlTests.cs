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
using Common.Models.ShortenUrl;
using FluentAssertions;
using Xunit;

public class ShortenUrlTests
{
    private readonly HttpClient _httpClient;
    private RequestShortenedUrlHandler _requestShortenedUrlHandler;
    private CreatePeerDidHandler _createPeerDidHandler;
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
        result.Value.ShortenedUrl.Should().NotBeNullOrEmpty();
        result.Value.ShortenedUrl.Should().Contain("/qr");
        result.Value.ShortenedUrl.Should().Contain("?_oobid=");

        // TODO test the rediection!
    }
}