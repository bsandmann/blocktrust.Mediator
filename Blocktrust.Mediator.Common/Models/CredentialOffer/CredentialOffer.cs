namespace Blocktrust.Mediator.Common.Models.CredentialOffer;

using System.Text.Json;
using DIDComm.Utils;
using FluentResults;
using PeerDID.Types;
using Pickup;
using Json = DIDComm.Message.Attachments.Json;

public class CredentialOffer
{
    public IssueCredentialOfferPreview IssueCredentialOfferPreview { get; set; }
    public List<Presentation> Presentations { get; set; }

    public string MessageId { get; set; }

    public PeerDid From { get; set; }
    public PeerDid To { get; set; }

    public CredentialOffer(IssueCredentialOfferPreview issueCredentialOfferPreview, List<Presentation> presentations, string messageId, PeerDid from, PeerDid to)
    {
        IssueCredentialOfferPreview = issueCredentialOfferPreview;
        Presentations = presentations;
        MessageId = messageId;
        From = from;
        To = to;
    }


    public static Result<CredentialOffer> ParseCredentialOffer(DeliveryResponseModel message)
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

        if (goalCode is null || !goalCode.Equals(GoalCodes.PrismCredentialOffer, StringComparison.InvariantCultureIgnoreCase))
        {
            {
                return Result.Fail($"Unexpected goal_code: {goalCode}");
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

        var issueCredentialOfferPreview = new IssueCredentialOfferPreview();
        if (message.Message.Body.TryGetValue("credential_preview", out var credentialPreviewJson))
        {
            var jsonElement = (JsonElement)credentialPreviewJson;
            if (jsonElement.ValueKind == JsonValueKind.Object)
            {
                foreach (var jsonProp in jsonElement.EnumerateObject())
                {
                    var jsonPropName = jsonProp.Name;
                    var jsonPropValue = jsonProp.Value;
                    if (jsonPropValue.ValueKind == JsonValueKind.String)
                    {
                        if (jsonPropName.Equals("type", StringComparison.InvariantCultureIgnoreCase))
                        {
                            issueCredentialOfferPreview.Type = jsonPropValue.GetString();
                        }
                    }

                    if (jsonPropValue.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var attribute in jsonPropValue.EnumerateArray())
                        {
                            if (attribute.ValueKind == JsonValueKind.Object)
                            {
                                var previewAttribute = new IssueCredentialOfferPreviewAttribute();
                                foreach (var keyValue in attribute.EnumerateObject())
                                {
                                    var key = keyValue.Name;
                                    if (keyValue.Value.ValueKind == JsonValueKind.String)
                                    {
                                        var value = keyValue.Value.GetString();
                                        if (key.Equals("name", StringComparison.InvariantCultureIgnoreCase))
                                        {
                                            previewAttribute.Name = value;
                                        }
                                        else if (key.Equals("mime-type", StringComparison.InvariantCultureIgnoreCase) || key.Equals("mimeType"))
                                        {
                                            previewAttribute.MimeType = value;
                                        }
                                        else if (key.Equals("value", StringComparison.InvariantCultureIgnoreCase))
                                        {
                                            previewAttribute.Value = value;
                                        }
                                    }
                                }

                                issueCredentialOfferPreview.Attributes.Add(previewAttribute);
                            }
                        }
                    }
                }
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
                    var format = formatEntity.GetString();
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
        var presentations = new List<Presentation>();
        foreach (var attachment in attachments!)
        {
            var id = attachment.Id ?? Guid.NewGuid().ToString();
            var data = attachment.Data;
            if (data is Json)
            {
                Json? jsonAttachmentData = (Json)data;
                var msg = jsonAttachmentData?.JsonString;
                var innerMessage = JsonSerializer.Serialize(msg, SerializationOptions.UnsafeRelaxedEscaping);
                var presentation = JsonSerializer.Deserialize<Presentation>(innerMessage, SerializationOptions.UnsafeRelaxedEscaping);
                presentations.Add(presentation);
            }
        }

        var messageId = message.Message!.Thid ?? message.MessageId;
        var from = message.Message.From?.Split('#').FirstOrDefault() ?? message.Metadata?.EncryptedFrom!.Split('#').FirstOrDefault();
        var tos = message.Message.To?.Select(p => p.Split('#').FirstOrDefault()).ToList() ?? message.Metadata?.EncryptedTo?.Select(p => p.Split('#').FirstOrDefault()).ToList()!;

        var fromPeerDid = new PeerDid(from!);
        var toPeerDid = new PeerDid(tos!.First());
        return Result.Ok(new CredentialOffer(issueCredentialOfferPreview, presentations, messageId, fromPeerDid, toPeerDid));
    }
}