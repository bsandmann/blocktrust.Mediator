namespace Blocktrust.Mediator.Common.Protocols;

using System.Text.Json;
using Blocktrust.Common.Resolver;
using DIDComm;
using DIDComm.Message.Messages;
using DIDComm.Model.PackEncryptedParamsModels;
using FluentResults;
using Models.Pickup;

public static class BasicMessage
{
    public static Message Create(string content, string from)
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
            .from(from)
            .build();
        return basicMessage;
    }

    public static async Task<Result<string>> Pack(Message basicMessage, string from, string finalRecipientDid, ISecretResolver secretResolver, IDidDocResolver didDocResolver)
    {
        var didComm = new DidComm(didDocResolver, secretResolver);
        var packResult = await didComm.PackEncrypted(
            new PackEncryptedParamsBuilder(basicMessage, to: finalRecipientDid)
                .From(from)
                .Forward(false) // When this flag is set to true (default), the Encryption Algo automatically searches for routingsKeys in the did and wraps it by itself
                .ProtectSenderId(false)
                .BuildPackEncryptedParams()
        );

        if (packResult.IsFailed)
        {
            return packResult.ToResult();
        }

        return packResult.Value.PackedMessage;
    }

    public static Result<BasicMessageContent> Parse(DeliveryResponseModel responseModel)
    {
        var message = responseModel.Message;
        var messageId = responseModel.MessageId;
        var metadata = responseModel.Metadata;

        if (message is null)
        {
            return Result.Fail("Message should not be null");
        }

        if (metadata is null)
        {
            return Result.Fail("Metadata should not be null");
        }

        if (!message.Type.Equals(ProtocolConstants.BasicMessage, StringComparison.InvariantCultureIgnoreCase))
        {
            return Result.Fail(string.Concat("Message is not a basic message. Type is '", message.Type, "'"));
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

        if (string.IsNullOrEmpty(messageId))
        {
            return Result.Fail("MessageId should not be empty");
        }

        if (message.From is null && metadata.EncryptedFrom is null)
        {
            return Result.Fail("From should not be null");
        }

        if ((message.To is null || !message.To.Any()) && (metadata.EncryptedTo is null || !metadata.EncryptedTo.Any()))
        {
            return Result.Fail("To should not be null or empty");
        }

        var basisMessageContent = new BasicMessageContent(
            message: contentJsonElement.GetString()!,
            messageId: messageId,
            from: message.From?.Split('#').FirstOrDefault() ?? metadata.EncryptedFrom!.Split('#').FirstOrDefault(),
            tos: message.To?.Select(p => p.Split('#').FirstOrDefault()).ToList() ?? metadata.EncryptedTo?.Select(p => p.Split('#').FirstOrDefault()).ToList()!,
            createdUtc: message.CreatedTime is not null ? DateTimeOffset.FromUnixTimeMilliseconds((long)message.CreatedTime).DateTime : DateTime.UtcNow
        );

        return Result.Ok(basisMessageContent);
    }
}