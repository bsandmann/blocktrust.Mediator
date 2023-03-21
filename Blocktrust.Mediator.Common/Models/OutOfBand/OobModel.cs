namespace Blocktrust.Mediator.Common.Models.OutOfBand;

using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Blocktrust.Common.Converter;
using Blocktrust.Mediator.Common.Protocols;
using Blocktrust.PeerDID.Types;

/// <summary>
/// Model for Out of band invitation: See https://identity.foundation/didcomm-messaging/spec/#out-of-band-messages
/// </summary>
public class OobModel
{
    /// <summary>
    /// REQUIRED. The header conveying the DIDComm MTURI: "https://didcomm.org/out-of-band/2.0/invitation"
    /// </summary>
    [JsonPropertyName("type")]
    public string Type { get; set; }

    /// <summary>
    ///  REQUIRED. This value MUST be used as the parent thread ID (pthid) for the response message that follows.
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; }

    /// <summary>
    /// REQUIRED for OOB usage. The DID representing the sender to be used by recipients for future interactions
    /// </summary>
    [JsonPropertyName("from")]
    public string From { get; set; }

    /// <summary>
    /// REQUIRED but body can be empty since the complete content is optiona
    /// </summary>
    [JsonPropertyName("body")]
    public OobBodyModel Body { get; set; }

    /// <summary>
    /// OPTIONAL. An array of attachments that will contain the invitation messages in order of preference that the receiver
    /// can use in responding to the message. Each message in the array is a rough equivalent of the others, and all are in
    /// pursuit of the stated goal and goal_code. Only one of the messages should be chosen and acted upon. 
    /// </summary>
    [JsonPropertyName("attachments")]
    public List<OobAttachmentModel>? Attachments { get; set; }

    /// <summary>
    /// Constructor for serialization
    /// </summary>
    public OobModel()
    {
    }

    /// <summary>
    /// Create a request-mediate-message
    /// </summary>
    /// <param name="from"></param>
    /// <returns></returns>
    public static string BuildRequestMediateOobMessage(PeerDid from)
    {
        var msg = new OobModel()
        {
            Type = ProtocolConstants.OutOfBand2Invitation,
            Id = Guid.NewGuid().ToString(),
            From = from.Value,
            Body = new OobBodyModel()
            {
                GoalCode = GoalCodes.RequestMediation,
                Goal = "Request mediate through the blocktrust mediator",
                Accept = new List<string>() { "didcomm/v2" }
            },
            Attachments = null
        };
        var jsonSerializerOptions = new JsonSerializerOptions()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            IgnoreNullValues = true
        };
        var json = JsonSerializer.Serialize(msg, jsonSerializerOptions);
        var base64Url = Base64Url.Encode(Encoding.UTF8.GetBytes(json));
        return base64Url;
    }

    public static string BuildGenericOobMessage(PeerDid from, string? goalCode = null, string? goal = null)
    {
        var msg = new OobModel()
        {
            Type = ProtocolConstants.OutOfBand2Invitation,
            Id = Guid.NewGuid().ToString(),
            From = from.Value,
            Body = new OobBodyModel()
            {
                GoalCode = goalCode,
                Goal = goal,
                Accept = new List<string>() { "didcomm/v2" }
            },
            Attachments = null
        };
        var jsonSerializerOptions = new JsonSerializerOptions()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            IgnoreNullValues = true
        };
        var json = JsonSerializer.Serialize(msg, jsonSerializerOptions);
        var base64Url = Base64Url.Encode(Encoding.UTF8.GetBytes(json));
        return base64Url;
    }
}