namespace Blocktrust.Mediator.Client.Commands.ShortenUrl.RequestShortenedUrl;

using System.Net;
using System.Text;
using System.Text.Json.Nodes;
using Blocktrust.Common.Resolver;
using Blocktrust.DIDComm;
using Blocktrust.DIDComm.Common.Types;
using Blocktrust.DIDComm.Message.Messages;
using Blocktrust.DIDComm.Model.PackEncryptedParamsModels;
using Blocktrust.DIDComm.Model.UnpackParamsModels;
using Blocktrust.Mediator.Common.Models.ShortenUrl;
using Blocktrust.Mediator.Common.Protocols;
using Common;
using FluentResults;
using MediatR;

public class RequestShortenedUrlHandler : IRequestHandler<RequestShortenedUrlRequest, Result<RequestShortenedUrlResponse>>
{
    private readonly HttpClient _httpClient;
    private readonly IDidDocResolver _didDocResolver;
    private readonly ISecretResolver _secretResolver;

    public RequestShortenedUrlHandler(HttpClient httpClient, IDidDocResolver didDocResolver, ISecretResolver secretResolver)
    {
        _httpClient = httpClient;
        _didDocResolver = didDocResolver;
        _secretResolver = secretResolver;
    }

    // Details: https://didcomm.org/shorten-url/1.0/

    public async Task<Result<RequestShortenedUrlResponse>> Handle(RequestShortenedUrlRequest request, CancellationToken cancellationToken)
    {
        // We create the message to send to the mediator
        // The special format of the return_route header is required by the python implementation of the roots mediator
        var returnRoute = new JsonObject() { new KeyValuePair<string, JsonNode?>("return_route", "all") };

        var body = new Dictionary<string, object>();
        body.Add("url", request.UrlToShorten);
        body.Add("requested_validity_seconds", request.RequestValidityInSeconds ?? 0);
        body.Add("goal_code", GoalCodes.ShortenOobv2);

        if (request.ShortUrlSlug is not null)
        {
            body.Add("short_url_slug", request.ShortUrlSlug);
        }

        var mediateRequestMessage = new MessageBuilder(
                id: Guid.NewGuid().ToString(),
                type: ProtocolConstants.ShortenedUrlRequest,
                body: body
            )
            .customHeader("custom_headers", new List<JsonObject>() { returnRoute })
            .build();

        var didComm = new DidComm(_didDocResolver, _secretResolver);

        // We pack the message and encrypt it for the mediator
        var packResult =await  didComm.PackEncrypted(
            new PackEncryptedParamsBuilder(mediateRequestMessage, to: request.MediatorDid)
                .From(request.LocalDid)
                .ProtectSenderId(false)
                .BuildPackEncryptedParams()
        );

        // We send the message to the mediator
        HttpResponseMessage response;
        try
        {
            response = await _httpClient.PostAsync(request.MediatorEndpoint, new StringContent(packResult.PackedMessage, Encoding.UTF8, MessageTyp.Encrypted), cancellationToken);
        }
        catch (HttpRequestException ex)
        {
            return Result.Fail($"Connection could not be established: {ex.Message}");
        }

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return Result.Fail("Connection could not be established");
        }
        else if (!response.IsSuccessStatusCode)
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

        if (unpackResult.Value.Message.Type == ProtocolConstants.ShortenedUrlResponse)
        {
            return RequestShortenedUrlResponse.Parse(unpackResult.Value.Message.Body!);
        }

        return Result.Fail("Error: Unexpected message response type");
    }
}