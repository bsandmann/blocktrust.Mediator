namespace Blocktrust.Mediator.Client.Commands.TrustPing;

using System.Net;
using System.Text;
using System.Text.Json.Nodes;
using Blocktrust.Common.Resolver;
using Blocktrust.DIDComm;
using Blocktrust.DIDComm.Common.Types;
using Blocktrust.DIDComm.Message.Messages;
using Blocktrust.DIDComm.Model.PackEncryptedParamsModels;
using Blocktrust.DIDComm.Model.UnpackParamsModels;
using Blocktrust.Mediator.Common.Protocols;
using Common;
using FluentResults;
using MediatR;
using PrismConnect;

public class PrismConnectHandler : IRequestHandler<PrismConnectRequest, Result<string>>
{
    private readonly HttpClient _httpClient;
    private readonly IDidDocResolver _didDocResolver;
    private readonly ISecretResolver _secretResolver;

    public PrismConnectHandler(HttpClient httpClient, IDidDocResolver didDocResolver, ISecretResolver secretResolver)
    {
        _httpClient = httpClient;
        _didDocResolver = didDocResolver;
        _secretResolver = secretResolver;
    }


    public async Task<Result<string>> Handle(PrismConnectRequest request, CancellationToken cancellationToken)
    {
        // The special format of the return_route header is required by the python implementation of the roots mediator
        var returnRoute = new JsonObject() { new KeyValuePair<string, JsonNode?>("return_route", "all") };
        var body = new Dictionary<string, object>();
        body.Add("goal_code", GoalCodes.PrismConnect);
        body.Add("goal", "Connect");
        body.Add("accept", "didcomm/v2");
        var mediateRequestMessage = new MessageBuilder(
                id: Guid.NewGuid().ToString(),
                type: ProtocolConstants.PrismConnectRequest,
                body: body
            )
            .customHeader("custom_headers", new List<JsonObject>() { returnRoute })
            .build();

        var didComm = new DidComm(_didDocResolver, _secretResolver);

        // We pack the message and encrypt it for the mediator
        var packResult =await  didComm.PackEncrypted(
            new PackEncryptedParamsBuilder(mediateRequestMessage, to: request.RemoteDid)
                .From(request.LocalDid)
                .ProtectSenderId(false)
                .BuildPackEncryptedParams()
        );

        // We send the message to the mediator
        HttpResponseMessage response;
        try
        {
            response = await _httpClient.PostAsync(request.RemoteEndpoint, new StringContent(packResult.PackedMessage, Encoding.UTF8, MessageTyp.Encrypted), cancellationToken);
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

        var unpackResult =await  didComm.Unpack(
            new UnpackParamsBuilder(content)
                .SecretResolver(_secretResolver)
                .BuildUnpackParams());
        if (unpackResult.IsFailed)
        {
            return unpackResult.ToResult();
        }

        var a = unpackResult.Value.Message.From;
        var b = unpackResult.Value.Message.FromPrior;
        var c = b.Iss;
        var d = b.Sub;
        var debugMsg = $"a: {a}, b: {b}, c: {c}, d: {d}";
        
        if (unpackResult.Value.Message.Type == ProtocolConstants.PrismConnectResponse)
        {
            return Result.Ok(debugMsg);
        }
        else
        {
            return Result.Fail("Error: Unexpected message response type");
        }
    }
}