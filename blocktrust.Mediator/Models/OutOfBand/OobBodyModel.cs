namespace Blocktrust.Mediator.Models;

using System.Text.Json.Serialization;

/// <summary>
/// Model for Out of band invitation: See https://identity.foundation/didcomm-messaging/spec/#out-of-band-messages
/// </summary>
public class OobBodyModel
{
    /// <summary>
    /// OPTIONAL. A self-attested code the receiver may want to display to the user or use in automatically
    /// deciding what to do with the out-of-band message.
    /// </summary>
    [JsonPropertyName("goal_code")] public string GoalCode { get; set; }
    /// <summary>
    /// OPTIONAL. A self-attested string that the receiver may want to display to the user about the
    /// context-specific goal of the out-of-band message.
    /// </summary>
    [JsonPropertyName("goal")] public string Goal { get; set; }
    /// <summary>
    /// OPTIONAL. An array of media types in the order of preference for sending a message to the endpoint.
    /// These identify a profile of DIDComm Messaging that the endpoint supports. If accept is not specified,
    /// the sender uses its preferred choice for sending a message to the endpoint.
    /// </summary>
    [JsonPropertyName("accept")] public List<string> Accept { get; set; }
}