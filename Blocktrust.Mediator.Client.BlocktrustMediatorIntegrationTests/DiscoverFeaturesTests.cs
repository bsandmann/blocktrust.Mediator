namespace Blocktrust.Mediator.Client.BlocktrustIntegrationTests;

using System.Text;
using System.Text.Json;
using Blocktrust.Common.Converter;
using Blocktrust.DIDComm.Secrets;
using Blocktrust.Mediator.Client.Commands.DiscoverFeatures;
using Blocktrust.Mediator.Client.Commands.MediatorCoordinator.QueryKeys;
using Blocktrust.Mediator.Common;
using Blocktrust.Mediator.Common.Commands.CreatePeerDid;
using Blocktrust.Mediator.Common.Models.DiscoverFeatures;
using Blocktrust.Mediator.Common.Models.OutOfBand;
using Blocktrust.PeerDID.DIDDoc;
using Blocktrust.PeerDID.PeerDIDCreateResolve;
using Blocktrust.PeerDID.Types;
using FluentAssertions;
using MediatR;
using Moq;
using Xunit;

public class DiscoverFeaturesTest
{
    private readonly HttpClient _httpClient;
    private DiscoverFeaturesHandler _discoverFeaturesHandler;
    private CreatePeerDidHandler _createPeerDidHandler;
    private readonly string _blocktrustMediatorUri = "https://localhost:7037/";

    public DiscoverFeaturesTest()
    {
        _httpClient = new HttpClient();
    }

    /// <summary>
    /// This tests assumes that the Blocktrust Mediator is running on https://localhost:7037/
    /// </summary>>
    [Fact]
    public async Task DiscoverFeatuesWithSimpleQueryMatchingEverything()
    {
        // Arrange
        // First get the OOB from the running mediator
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
        var request = new DiscoverFeaturesRequest(new Uri(mediatorEndpoint.Uri),mediatorDid, localDid.Value.PeerDid.Value, queries);

        // Act
        _discoverFeaturesHandler = new DiscoverFeaturesHandler(_httpClient, new SimpleDidDocResolver(), secretResolverInMemory);
        var result = await _discoverFeaturesHandler.Handle(request, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Count.Should().Be(7);
        result.Value[0].FeatureType.Should().Be("protocol");
        result.Value[0].Id.Should().Be("https://didcomm.org/out-of-band/2.0");
    }
    
    /// <summary>
    /// This tests assumes that the Blocktrust Mediator is running on https://localhost:7037/
    /// </summary>>
    [Fact]
    public async Task DiscoverFeatuesWithSimpleQueryMatchingOnlyCoordinateMediation()
    {
        // Arrange
        // First get the OOB from the running mediator
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
        queries.Add(new FeatureQuery("protocol","https://didcomm.org/coordinate-mediation/2.0"));
        var request = new DiscoverFeaturesRequest(new Uri(mediatorEndpoint.Uri),mediatorDid, localDid.Value.PeerDid.Value, queries);

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
    /// This tests assumes that the Blocktrust Mediator is running on https://localhost:7037/
    /// </summary>>
    [Fact]
    public async Task DiscoverFeatuesWithSimpleQueryMatchingOnlyCoordinateMediationStar()
    {
        // Arrange
        // First get the OOB from the running mediator
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
        queries.Add(new FeatureQuery("protocol","https://didcomm.org/coordinate-mediation.*"));
        var request = new DiscoverFeaturesRequest(new Uri(mediatorEndpoint.Uri),mediatorDid, localDid.Value.PeerDid.Value, queries);

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