namespace Blocktrust.Mediator.Client.Commands.MediatorCoordinator.UpdateKeys;

using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Blocktrust.Common.Resolver;
using Blocktrust.DIDComm;
using Blocktrust.DIDComm.Common.Types;
using Blocktrust.DIDComm.Message.Messages;
using Blocktrust.DIDComm.Model.PackEncryptedParamsModels;
using Blocktrust.DIDComm.Model.UnpackParamsModels;
using Blocktrust.Mediator.Common.Protocols;
using Common.Models.MediatorCoordinator;
using FluentResults;
using MediatR;
using QueryKeys;

public class QueryMediatorKeysHandler : IRequestHandler<QueryMediatorKeysRequest, Result<List<string>>>
{
    private readonly IMediator _mediator;
    private readonly HttpClient _httpClient;
    private readonly IDidDocResolver _didDocResolver;
    private readonly ISecretResolver _secretResolver;

    public QueryMediatorKeysHandler(IMediator mediator, HttpClient httpClient, IDidDocResolver didDocResolver, ISecretResolver secretResolver)
    {
        _mediator = mediator;
        _httpClient = httpClient;
        _didDocResolver = didDocResolver;
        _secretResolver = secretResolver;
    }

    public async Task<Result<List<string>>> Handle(QueryMediatorKeysRequest request, CancellationToken cancellationToken)
    {
        // We create the message to send to the mediator
        // See: https://didcomm.org/mediator-coordination/2.0/
        // The special format of the return_route header is required by the python implementation of the roots mediator
        var returnRoute = new JsonObject() { new KeyValuePair<string, JsonNode?>("return_route", "all") };
        var mediateRequestMessage = new MessageBuilder(
                id: Guid.NewGuid().ToString(),
                type: ProtocolConstants.CoordinateMediation2KeylistQuery,
                body: new Dictionary<string, object>()
            )
            .customHeader("custom_headers", new List<JsonObject>() { returnRoute })
            .build();

        var didComm = new DidComm(_didDocResolver, _secretResolver);

        // We pack the message and encrypt it for the mediator
        var packResult = didComm.PackEncrypted(
            new PackEncryptedParamsBuilder(mediateRequestMessage, to: request.MediatorDid)
                .From(request.LocalDid)
                .ProtectSenderId(false)
                .BuildPackEncryptedParams()
        );

        // We send the message to the mediator
        var response = await _httpClient.PostAsync(request.MediatorEndpoint, new StringContent(packResult.PackedMessage, Encoding.UTF8, MessageTyp.Encrypted), cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return Result.Fail("Connection could not be established");
        }
        else if (!response.IsSuccessStatusCode)
        {
            return Result.Fail("Unable to initiate connection: " + response.StatusCode);
        }

        var content = await response.Content.ReadAsStringAsync(cancellationToken);

        var unpackResult = didComm.Unpack(
            new UnpackParamsBuilder(content)
                .SecretResolver(_secretResolver)
                .BuildUnpackParams());
        if (unpackResult.IsFailed)
        {
            return unpackResult.ToResult();
        }

        if (unpackResult.Value.Message.Type == ProtocolConstants.CoordinateMediation2KeylistQueryResponse)
        {
            var body = unpackResult.Value.Message.Body;
            if (body.ContainsKey("keys"))
            {
                var hasKeys = body.TryGetValue("keys", out var keys);
                if (hasKeys)
                {
                    var keysAsJsonElement = (JsonElement)keys;
                    var returnList = new List<string>();
                    if (keysAsJsonElement.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var entry in  keysAsJsonElement.EnumerateArray())
                        {
                            entry.TryGetProperty("recipient_did", out var value);
                            returnList.Add(value.GetString());
                        }
                        return Result.Ok(returnList);
                    }
                }
            }

            return Result.Fail("Unexpected body content");
        }
        else
        {
            return Result.Fail("Error: Unexpected message response type");
        }
    }
}