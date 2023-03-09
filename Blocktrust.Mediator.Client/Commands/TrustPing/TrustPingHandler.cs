﻿namespace Blocktrust.Mediator.Client.Commands.MediatorCoordinator.QueryKeys;

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
using Common.Models.DiscoverFeatures;
using DiscoverFeatures;
using FluentResults;
using MediatR;

public class TrustPingHandler : IRequestHandler<TrustPingRequest, Result>
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

    public async Task<Result> Handle(TrustPingRequest request, CancellationToken cancellationToken)
    {
        //TODO in general I should allow the option to use the return_route feature or not
        //And i should also use both header-implementations!
        
        // We create the message to send to the mediator
        // The special format of the return_route header is required by the python implementation of the roots mediator
        var returnRoute = new JsonObject() { new KeyValuePair<string, JsonNode?>("return_route", "all") };
        var mediateRequestMessage = new MessageBuilder(
                id: Guid.NewGuid().ToString(),
                type: ProtocolConstants.TrustPingRequest,
                body: new Dictionary<string, object> { { "response_requested", request.ResponseRequested } }
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

        if (unpackResult.Value.Message.Type == ProtocolConstants.TrustPingResponse)
        {
            return Result.Ok();
        }
        else
        {
            return Result.Fail("Error: Unexpected message response type");
        }
    }
}