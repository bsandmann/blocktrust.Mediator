namespace Blocktrust.Mediator.Client.Commands.DiscoverFeatures;

using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json.Nodes;
using Blocktrust.Common.Resolver;
using Blocktrust.DIDComm;
using Blocktrust.DIDComm.Common.Types;
using Blocktrust.DIDComm.Message.Messages;
using Blocktrust.DIDComm.Model.PackEncryptedParamsModels;
using Blocktrust.DIDComm.Model.UnpackParamsModels;
using Blocktrust.Mediator.Common.Models.DiscoverFeatures;
using Blocktrust.Mediator.Common.Protocols;
using FluentResults;
using MediatR;

public class DiscoverFeaturesHandler : IRequestHandler<DiscoverFeaturesRequest, Result<List<DiscoverFeature>>>
{
    private readonly HttpClient _httpClient;
    private readonly IDidDocResolver _didDocResolver;
    private readonly ISecretResolver _secretResolver;

    public DiscoverFeaturesHandler(HttpClient httpClient, IDidDocResolver didDocResolver, ISecretResolver secretResolver)
    {
        _httpClient = httpClient;
        _didDocResolver = didDocResolver;
        _secretResolver = secretResolver;
    }

    public async Task<Result<List<DiscoverFeature>>> Handle(DiscoverFeaturesRequest request, CancellationToken cancellationToken)
    {
        try
        {
            // Create the message to send to the mediator
            var returnRoute = new JsonObject() { new KeyValuePair<string, JsonNode?>("return_route", "all") };
            var body = new Dictionary<string, object>
            {
                { "queries", request.Queries }
            };

            var mediateRequestMessage = new MessageBuilder(
                    id: Guid.NewGuid().ToString(),
                    type: ProtocolConstants.DiscoverFeatures2Query,
                    body: body
                )
                .to(new List<string>() { request.MediatorDid })
                .returnRoute("all")
                .customHeader("custom_headers", new List<JsonObject>() { returnRoute })
                .from(request.LocalDid)
                .build();

            var didComm = new DidComm(_didDocResolver, _secretResolver);

            // Pack the message with explicit resolvers and forwarding disabled
            var packResult = await didComm.PackEncrypted(
                new PackEncryptedParamsBuilder(mediateRequestMessage, to: request.MediatorDid)
                    .From(request.LocalDid)
                    .ProtectSenderId(false)
                    .BuildPackEncryptedParams()
            );

            if (packResult.IsFailed)
            {
                return Result.Fail($"Message packing failed: {string.Join(", ", packResult.Errors.Select(e => e.Message))}");
            }

            // Send the message to the mediator
            HttpResponseMessage response;
            try
            {
                response = await _httpClient.PostAsync(request.MediatorEndpoint, 
                    new StringContent(packResult.Value.PackedMessage, new MediaTypeHeaderValue(MessageTyp.Encrypted)), 
                    cancellationToken);
            }
            catch (HttpRequestException ex)
            {
                return Result.Fail($"Connection could not be established: {ex.Message}");
            }

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return Result.Fail("Connection could not be established");
            }

            if (!response.IsSuccessStatusCode)
            {
                return Result.Fail("Unable to initiate connection: " + response.StatusCode);
            }

            var content = await response.Content.ReadAsStringAsync(cancellationToken);

            var unpackResult = await didComm.Unpack(
                new UnpackParamsBuilder(content)
                    .SecretResolver(_secretResolver)
                    .BuildUnpackParams());

            if (unpackResult.IsFailed)
            {
                return Result.Fail($"Message unpacking failed: {string.Join(", ", unpackResult.Errors.Select(e => e.Message))}");
            }

            if (unpackResult.Value.Message.Type == ProtocolConstants.DiscoverFeatures2Response)
            {
                return DiscoverFeature.Parse(unpackResult.Value.Message.Body);
            }

            return Result.Fail($"Unexpected message response type: {unpackResult.Value.Message.Type}");
        }
        catch (Exception ex)
        {
            return Result.Fail($"Error during feature discovery: {ex.Message}");
        }
    }
}