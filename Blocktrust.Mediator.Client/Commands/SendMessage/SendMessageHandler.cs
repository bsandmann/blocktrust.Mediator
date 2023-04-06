namespace Blocktrust.Mediator.Client.Commands.SendMessage;

using System.Net;
using System.Net.Http.Headers;
using System.Text;
using Blocktrust.Common.Resolver;
using Blocktrust.DIDComm;
using Blocktrust.DIDComm.Common.Types;
using Blocktrust.DIDComm.Message.Messages;
using Blocktrust.DIDComm.Model.PackEncryptedParamsModels;
using Blocktrust.DIDComm.Model.UnpackParamsModels;
using FluentResults;
using MediatR;

//TODO this is very generic implementation of message sending; we might use it in all other commands

public class SendMessageHandler : IRequestHandler<SendMessageRequest, Result<Message>>
{
    private readonly HttpClient _httpClient;
    private readonly IDidDocResolver _didDocResolver;
    private readonly ISecretResolver _secretResolver;

    public SendMessageHandler(HttpClient httpClient, IDidDocResolver didDocResolver, ISecretResolver secretResolver)
    {
        _httpClient = httpClient;
        _didDocResolver = didDocResolver;
        _secretResolver = secretResolver;
    }

    public async Task<Result<Message>> Handle(SendMessageRequest request, CancellationToken cancellationToken)
    {
        var didComm = new DidComm(_didDocResolver, _secretResolver);

        // We pack the message and encrypt it for the mediator
        var packResult = await didComm.PackEncrypted(
            new PackEncryptedParamsBuilder(request.Message, to: request.RemoteDid)
                .From(request.LocalDid)
                .ProtectSenderId(false)
                .BuildPackEncryptedParams()
        );

        if (packResult.IsFailed)
        {
            return packResult.ToResult();
        }

        // We send the message to the endpoint
        HttpResponseMessage response;
        try
        {
            response = await _httpClient.PostAsync(request.RemoteEndpoint, new StringContent(packResult.Value.PackedMessage, new MediaTypeHeaderValue(MessageTyp.Encrypted)), cancellationToken);
        }
        catch (HttpRequestException ex)
        {
            return Result.Fail($"Connection could not be established: {ex.Message}");
        }

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return Result.Fail("Connection could not be established. Not found");
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

            return Result.Ok(unpackResult.Value.Message);
        }

        return Result.Ok();
    }
}