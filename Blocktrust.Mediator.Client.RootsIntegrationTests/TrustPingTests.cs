namespace Blocktrust.Mediator.Client.RootsIntegrationTests;

using System.Text;
using System.Text.Json;
using Blocktrust.Common.Converter;
using Blocktrust.Mediator.Common.Commands.CreatePeerDid;
using Commands.DiscoverFeatures;
using Commands.MediatorCoordinator.QueryKeys;
using Commands.TrustPing;
using Common;
using Common.Models.OutOfBand;
using DIDComm.Secrets;
using FluentAssertions;
using PeerDID.DIDDoc;
using PeerDID.PeerDIDCreateResolve;
using PeerDID.Types;
using Xunit;

public class TrustPingTests
{
    private readonly HttpClient _httpClient;
    private TrustPingHandler _trustPingHandler;
    private CreatePeerDidHandler _createPeerDidHandlerAlice;

    public TrustPingTests()
    {
        _httpClient = new HttpClient();
    }

    /// <summary>
    /// This tests assumes that the Roots Mediator is running on http://127.0.0.1:8000
    /// </summary>
    [Fact]
    public async Task TrustPingTest()
    {
        var oobInvitationRootsLocal =
            "eyJ0eXBlIjoiaHR0cHM6Ly9kaWRjb21tLm9yZy9vdXQtb2YtYmFuZC8yLjAvaW52aXRhdGlvbiIsImlkIjoiNGZlYjY4NjctMGRkYS00MWRkLWJiYjUtNWU5MDJjZDZjNzdkIiwiZnJvbSI6ImRpZDpwZWVyOjIuRXo2TFNxSmZnYkU5QTJBbUI2UkpmcEpDZ1J6b2pLUmdNRHNKM0hhMXlWandxcG1TQi5WejZNa29meVZiYWJDSnNGb1BxMjZNUmU3bURaQ2hxaE0xRzJEamtNek5KUEVRYVgxLlNleUpwWkNJNkltNWxkeTFwWkNJc0luUWlPaUprYlNJc0luTWlPaUpvZEhSd09pOHZNVEkzTGpBdU1DNHhPamd3TURBaUxDSmhJanBiSW1ScFpHTnZiVzB2ZGpJaVhYMCIsImJvZHkiOnsiZ29hbF9jb2RlIjoicmVxdWVzdC1tZWRpYXRlIiwiZ29hbCI6IlJlcXVlc3RNZWRpYXRlIiwibGFiZWwiOiJNZWRpYXRvciIsImFjY2VwdCI6WyJkaWRjb21tL3YyIl19fQ";

        var secretResolverInMemoryForAlice = new SecretResolverInMemory();
        var simpleDidDocResolverForAlice = new SimpleDidDocResolver();
        _createPeerDidHandlerAlice = new CreatePeerDidHandler(secretResolverInMemoryForAlice);

        var decodedInvitation = Encoding.UTF8.GetString(Base64Url.Decode(oobInvitationRootsLocal));
        var oobModel = JsonSerializer.Deserialize<OobModel>(decodedInvitation);
        var invitationPeerDidResult = PeerDidResolver.ResolvePeerDid(new PeerDid(oobModel.From), VerificationMaterialFormatPeerDid.Jwk);
        var invitationPeerDidDocResult = DidDocPeerDid.FromJson(invitationPeerDidResult.Value);

        var mediatorDid = invitationPeerDidDocResult.Value.Did;
        var mediatorEndpoint = invitationPeerDidDocResult.Value.Services.FirstOrDefault().ServiceEndpoint;

        var localDidOfAliceToUseWithTheMediator = await _createPeerDidHandlerAlice.Handle(new CreatePeerDidRequest(), cancellationToken: new CancellationToken());
        var request = new TrustPingRequest(new Uri(mediatorEndpoint), mediatorDid, localDidOfAliceToUseWithTheMediator.Value.PeerDid.Value, true);

        _trustPingHandler = new TrustPingHandler(_httpClient, simpleDidDocResolverForAlice, secretResolverInMemoryForAlice);
        var trustPingResult = await _trustPingHandler.Handle(request, CancellationToken.None);

        trustPingResult.IsSuccess.Should().BeTrue();
    }
}