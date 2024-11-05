using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Nodes;
using Blocktrust.Common.Resolver;
using Blocktrust.DIDComm;
using Blocktrust.DIDComm.Common.Types;
using Blocktrust.DIDComm.Message.Messages;
using Blocktrust.DIDComm.Model.PackEncryptedParamsModels;
using Blocktrust.DIDComm.Model.UnpackParamsModels;
using Blocktrust.Mediator.Common.Protocols;
using FluentResults;
using MediatR;

namespace Blocktrust.Mediator.Client.Commands.TrustPing;

public class TrustPingHandler : IRequestHandler<TrustPingRequest, Result<string?>>
{
    private readonly HttpClient _httpClient;
    private readonly IDidDocResolver _didDocResolver;
    private readonly ISecretResolver _secretResolver;

    public TrustPingHandler(HttpClient httpClient, IDidDocResolver didDocResolver, ISecretResolver secretResolver)
    {
        _httpClient = httpClient;
        _didDocResolver = didDocResolver;
        _secretResolver = secretResolver;
    }

    // Details: https://identity.foundation/didcomm-messaging/spec/#discover-features-protocol-20
    // https://didcomm.org/discover-features/2.0/

public async Task<Result<string?>> Handle(TrustPingRequest request, CancellationToken cancellationToken)
{
    //TODO in general I should allow the option to use the return_route feature or not
    //And i should also use both header-implementations!

    // We create the message to send to the mediator
    // The special format of the return_route header is required by the python implementation of the roots mediator
    try
    {
        var returnRoute = new JsonObject { new KeyValuePair<string, JsonNode?>("return_route", "all") };
        var body = new Dictionary<string, object>
        {
            { "response_requested", request.ResponseRequested },
        };
        if (request.SuggestedLabel is not null)
        {
            body.Add("suggested_label", request.SuggestedLabel);
        }

        var mediateRequestMessage = new MessageBuilder(
                id: Guid.NewGuid().ToString(),
                type: ProtocolConstants.TrustPingRequest,
                body: body
            )
            .to(new List<string> { request.RemoteDid })
            .returnRoute("all")
            .customHeader("custom_headers", new List<JsonObject> { returnRoute })
            .from(request.LocalDid)
            .build();

        var didComm = new DidComm(_didDocResolver, _secretResolver);

        // Pack the message with forwarding disabled
        var packResult = await didComm.PackEncrypted(
            new PackEncryptedParamsBuilder(mediateRequestMessage, to: request.RemoteDid)
                .From(request.LocalDid)
                .ProtectSenderId(false)
                .DidDocResolver(_didDocResolver)
                .SecretResolver(_secretResolver)
                .Forward(false)  // Explicitly disable forwarding
                .BuildPackEncryptedParams()
        );

        if (packResult.IsFailed)
        {
            return Result.Fail($"Message packing failed: {string.Join(", ", packResult.Errors.Select(e => e.Message))}");
        }

        var response = await _httpClient.PostAsync(
            request.RemoteEndpoint, 
            new StringContent(packResult.Value.PackedMessage, new MediaTypeHeaderValue(MessageTyp.Encrypted)), 
            cancellationToken);

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

        if (unpackResult.Value.Message.Type == ProtocolConstants.TrustPingResponse)
        {
            string? label = null;
            if (unpackResult.Value.Message.Body.ContainsKey("suggested_label"))
            {
                var labelJsonElement = (JsonElement)unpackResult.Value.Message.Body["suggested_label"];
                if (labelJsonElement.ValueKind == JsonValueKind.String)
                {
                    label = labelJsonElement.GetString();
                }
            }
            else if (unpackResult.Value.Message.Body.ContainsKey("label"))
            {
                var labelJsonElement = (JsonElement)unpackResult.Value.Message.Body["label"];
                if (labelJsonElement.ValueKind == JsonValueKind.String)
                {
                    label = labelJsonElement.GetString();
                }
            }

            return Result.Ok(label);
        }

        return Result.Fail($"Unexpected message response type: {unpackResult.Value.Message.Type}");
    }
    catch (Exception ex)
    {
        return Result.Fail($"Error during trust ping: {ex.Message}");
    }
}
}