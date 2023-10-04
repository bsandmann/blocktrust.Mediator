namespace Blocktrust.Mediator.Client.Commands.ShortenUrl.InvalidateShortenedUrl;

using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json.Nodes;
using Blocktrust.Common.Resolver;
using Blocktrust.DIDComm;
using Blocktrust.DIDComm.Common.Types;
using Blocktrust.DIDComm.Message.Messages;
using Blocktrust.DIDComm.Model.PackEncryptedParamsModels;
using Blocktrust.Mediator.Common.Protocols;
using Common.Models.ProblemReport;
using DIDComm.Model.UnpackParamsModels;
using FluentResults;
using MediatR;

public class InvalidateShortenedUrlHandler : IRequestHandler<InvalidateShortenedUrlRequest, Result<ProblemReport>>
{
    private readonly HttpClient _httpClient;
    private readonly IDidDocResolver _didDocResolver;
    private readonly ISecretResolver _secretResolver;

    public InvalidateShortenedUrlHandler(HttpClient httpClient, IDidDocResolver didDocResolver, ISecretResolver secretResolver)
    {
        _httpClient = httpClient;
        _didDocResolver = didDocResolver;
        _secretResolver = secretResolver;
    }

    // Details: https://didcomm.org/shorten-url/1.0/

    public async Task<Result<ProblemReport>> Handle(InvalidateShortenedUrlRequest request, CancellationToken cancellationToken)
    {
        // We create the message to send to the mediator
        // The special format of the return_route header is required by the python implementation of the roots mediator
        var returnRoute = new JsonObject() { new KeyValuePair<string, JsonNode?>("return_route", "all") };

        var body = new Dictionary<string, object>();
        body.Add("shortened_url", request.ShortenedUrl);
        var mediateRequestMessage = new MessageBuilder(
                id: Guid.NewGuid().ToString(),
                type: ProtocolConstants.InvalidateShortenedUrl,
                body: body
            )
            .returnRoute("all")
            .customHeader("custom_headers", new List<JsonObject>() { returnRoute })
            .from(request.LocalDid)
            .build();

        var didComm = new DidComm(_didDocResolver, _secretResolver);

        // We pack the message and encrypt it for the mediator
        var packResult = await didComm.PackEncrypted(
            new PackEncryptedParamsBuilder(mediateRequestMessage, to: request.MediatorDid)
                .From(request.LocalDid)
                .ProtectSenderId(false)
                .BuildPackEncryptedParams()
        );
        
        if(packResult.IsFailed)
        {
            return packResult.ToResult();
        }

        // We send the message to the mediator
        HttpResponseMessage response;
        try
        {
            response = await _httpClient.PostAsync(request.MediatorEndpoint, new StringContent(packResult.Value.PackedMessage, new MediaTypeHeaderValue(MessageTyp.Encrypted) ), cancellationToken);
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
        
        if (!string.IsNullOrEmpty(content))
        {
            var unpackResult = await didComm.Unpack(
                new UnpackParamsBuilder(content)
                    .SecretResolver(_secretResolver)
                    .BuildUnpackParams());
            if (unpackResult.IsFailed)
            {
                return unpackResult.ToResult();
            }

            if (unpackResult.Value.Message.Type == ProtocolConstants.ProblemReport)
            {
                if (unpackResult.Value.Message.Pthid != null)
                {
                    var problemReport = ProblemReport.Parse(unpackResult.Value.Message.Body, unpackResult.Value.Message.Pthid);
                    if (problemReport.IsFailed)
                    {
                        return Result.Fail("Error parsing the problem report of the mediator");
                    }

                    return Result.Ok(problemReport.Value);
                }

                return Result.Fail("Error parsing the problem report of the mediator. Missing parent-thread-id");
            }
        }

        return Result.Ok();
    }
}