namespace Blocktrust.Mediator.Common.Protocols;

using System.Text.Json;
using Blocktrust.Common.Resolver;
using DIDComm;
using DIDComm.Message.Messages;
using DIDComm.Model.PackEncryptedParamsModels;
using FluentResults;

public static class BasicMessage
{
    public static Message Create(string content)
    {
        var basicMessage = new MessageBuilder(
                id: Guid.NewGuid().ToString(),
                type: ProtocolConstants.BasicMessage,
                body: new Dictionary<string, object>()
                {
                    { "content", content }
                }
            )
            .customHeader("lang", "en")
            .createdTime(new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds())
            .build();
        return basicMessage;
    }

    public static async Task<string> Pack(Message basicMessage, string from, string to, ISecretResolver secretResolver, IDidDocResolver didDocResolver)
    {
        var didComm = new DidComm(didDocResolver, secretResolver);
        var packResult =await  didComm.PackEncrypted(
            new PackEncryptedParamsBuilder(basicMessage, to: to)
                .From(from)
                .ProtectSenderId(false)
                .BuildPackEncryptedParams()
        );

        return packResult.PackedMessage;
    }

    public static Result<string> Parse(Message message)
    {
        if (message.Type != ProtocolConstants.BasicMessage)
        {
            return Result.Fail("Message is not a basic message");
        }

        if (!message.Body.ContainsKey("content"))
        {
            return Result.Fail("Message does not contain a content field");
        }

        var contentJsonElement = (JsonElement)message.Body["content"]!;
        if (contentJsonElement.ValueKind != JsonValueKind.String)
        {
            return Result.Fail("Message content is not a string");
        }

        return Result.Ok(contentJsonElement!.GetString());
    }
}