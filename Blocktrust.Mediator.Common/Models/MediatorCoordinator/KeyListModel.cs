namespace Blocktrust.Mediator.Common.Models.MediatorCoordinator;

using System.Text.Json.Serialization;

public class KeyListModel
{
    [JsonPropertyName("recipient_did")] public string QueryResult { get; set; }
}