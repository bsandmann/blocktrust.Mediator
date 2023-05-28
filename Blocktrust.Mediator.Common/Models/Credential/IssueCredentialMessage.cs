namespace Blocktrust.Mediator.Common.Models.Credential;

using System.Text;
using System.Text.Json;
using Blocktrust.Common.Converter;
using Blocktrust.DIDComm.Message.Attachments;
using Blocktrust.Mediator.Common.Models.Pickup;
using Blocktrust.PeerDID.Types;
using FluentResults;

public class IssueCredentialMessage
{
    public PeerDid? From { get; }
    public PeerDid To { get; }
    public string MessageId { get; }
    public List<Credential> Credentials { get; }

    public IssueCredentialMessage(PeerDid from, PeerDid to, string messageId, List<Credential> credentials)
    {
        From = from;
        To = to;
        MessageId = messageId;
        Credentials = credentials;
    }


    public static Result<IssueCredentialMessage> Process(DeliveryResponseModel message)
    {
        if (message.Message is null)
        {
            return Result.Fail("Unable to parse model without message");
        }

        string? goalCode = null;
        if (message.Message.Body.TryGetValue("goal_code", out var goalCodeJson))
        {
            var jsonElement = (JsonElement)goalCodeJson;
            if (jsonElement.ValueKind == JsonValueKind.String)
            {
                goalCode = jsonElement.GetString();
            }
        }

        // currently not used by PRISM
        string? multipleAvailable = null;
        if (message.Message.Body.TryGetValue("multiple_available", out var multipleAvailableJson))
        {
            var jsonElement = (JsonElement)multipleAvailableJson;
            if (jsonElement.ValueKind == JsonValueKind.Number)
            {
                multipleAvailable = jsonElement.GetString();
            }
        }

        // currently not used by PRISM
        string? replacementId = null;
        if (message.Message.Body.TryGetValue("replacement_id", out var replacementIdJson))
        {
            var jsonElement = (JsonElement)replacementIdJson;
            if (jsonElement.ValueKind == JsonValueKind.String)
            {
                replacementId = jsonElement.GetString();
            }
        }

        // currently not used by PRISM
        string? formats = null;
        if (message.Message.Body.TryGetValue("formats", out var formatsJson))
        {
            var jsonElement = (JsonElement)formatsJson;
            if (jsonElement.ValueKind == JsonValueKind.String)
            {
                formats = jsonElement.GetString();
            }

            if (jsonElement.ValueKind == JsonValueKind.Array)
            {
                foreach (var formatEntity in jsonElement.EnumerateArray())
                {
                    if (formatEntity.ValueKind == JsonValueKind.String)
                    {
                        var format = formatEntity.GetString();
                    }
                }
            }
        }

        string? comment = null;
        if (message.Message.Body.TryGetValue("comment", out var commentJson))
        {
            var jsonElement = (JsonElement)commentJson;
            if (jsonElement.ValueKind == JsonValueKind.String)
            {
                comment = jsonElement.GetString();
            }
        }

        var attachments = message.Message.Attachments;
        var credentials = new List<Credential>();
        foreach (var attachment in attachments!)
        {
            var id = attachment.Id ?? Guid.NewGuid().ToString();
            var data = attachment.Data;
            if (data is Base64)
            {
                Base64? base64AttachmentData = (Base64)data;
                var base64String = base64AttachmentData?.Base64String;
                var base64Decode = Base64Url.Decode(base64String);
                var jwt = Encoding.UTF8.GetString(base64Decode);

                var credential = new Credential().Parse(jwt);
                credentials.Add(credential);
            }
        }

        var messageId = message.Message!.Thid ?? message.MessageId;
        var from = message.Message.From?.Split('#').FirstOrDefault() ?? message.Metadata?.EncryptedFrom!.Split('#').FirstOrDefault();
        var tos = message.Message.To?.Select(p => p.Split('#').FirstOrDefault()).ToList() ?? message.Metadata?.EncryptedTo?.Select(p => p.Split('#').FirstOrDefault()).ToList()!;

        var fromPeerDid = new PeerDid(from!);
        var toPeerDid = new PeerDid(tos!.First());

        return Result.Ok(new IssueCredentialMessage(fromPeerDid, toPeerDid, messageId, credentials));
    }
}