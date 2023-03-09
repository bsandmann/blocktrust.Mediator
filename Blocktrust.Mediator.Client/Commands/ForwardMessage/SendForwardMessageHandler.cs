namespace Blocktrust.Mediator.Client.Commands.ForwardMessage;

using System.Net;
using System.Text;
using Blocktrust.Common.Resolver;
using Common.Protocols;
using DIDComm;
using DIDComm.Common.Types;
using DIDComm.Message.Attachments;
using DIDComm.Message.Messages;
using DIDComm.Model.PackEncryptedParamsModels;
using FluentResults;
using MediatR;
using Json = DIDComm.Message.Attachments.Json;

public class SendForwardMessageHandler : IRequestHandler<SendForwardMessageRequest, Result>
{
    private readonly HttpClient _httpClient;
    private readonly IDidDocResolver _didDocResolver;
    private readonly ISecretResolver _secretResolver;

    public SendForwardMessageHandler(HttpClient httpClient, IDidDocResolver didDocResolver, ISecretResolver secretResolver)
    {
        _httpClient = httpClient;
        _didDocResolver = didDocResolver;
        _secretResolver = secretResolver;
    }

    public async Task<Result> Handle(SendForwardMessageRequest request, CancellationToken cancellationToken)
    {
        // We create the wrapping message, with has the inner message in the attachments
        Dictionary<string, object> packedMessage;
        try
        {
            packedMessage = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(request.Message);
        }
        catch (Exception e)
        {
            return Result.Fail($"The message cannot be deserialized: {e}");
        }

        if (packedMessage is null)
        {
            return Result.Fail("The message cannot be deserialized");
        }

        var attachments = new List<Attachment>
        {
            new AttachmentBuilder(
                id: Guid.NewGuid().ToString(),
                data: new Json(json: packedMessage)
            ).Build()
        };
        var wrappedMessage = new MessageBuilder(
                id: Guid.NewGuid().ToString(),
                type: ProtocolConstants.ForwardMessage,
                body: new Dictionary<string, object>()
                {
                    { "next", request.RecipientDid }
                }
            )
            .attachments(attachments)
            .to(new List<string>() { request.MediatorDid })
            .build();


        var didComm = new DidComm(_didDocResolver, _secretResolver);

        // We pack the message and encrypt it for the mediator
        var packResult = didComm.PackEncrypted(
            new PackEncryptedParamsBuilder(wrappedMessage, to: request.MediatorDid)
                .From(request.LocalDid)
                .ProtectSenderId(false)
                .BuildPackEncryptedParams()
        );

        // We send the message to the mediator endpoint
        var response = await _httpClient.PostAsync(request.MediatorEndpoint, new StringContent(packResult.PackedMessage, Encoding.UTF8, MessageTyp.Encrypted), cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return Result.Fail("Connection could not be established");
        }
        else if (!response.IsSuccessStatusCode)
        {
            return Result.Fail("Unable to initiate connection: " + response.StatusCode);
        }
        else if (response.StatusCode == HttpStatusCode.Accepted)
        {
            return Result.Ok();
        }
        else
        {
            return Result.Fail("The result code should be 202! This is not really a fail here, but anyway....");
        }
    }
}