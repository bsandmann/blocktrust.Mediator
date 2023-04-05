namespace Blocktrust.Mediator.Common.Models.MediatorCoordinator;

using System.Text.Json.Serialization;

public class KeyListUpdateModel
{
    [JsonPropertyName("recipient_did")] public string KeyToUpdate { get; set; }
    [JsonPropertyName("action")] public string UpdateType { get; set; }

    /// <summary>
    /// For serialization / deserialization
    /// </summary>
    [JsonConstructor]
    public KeyListUpdateModel()
    {
    }

    public KeyListUpdateModel(string keyToUpdate, bool addKey)
    {
        KeyToUpdate = keyToUpdate;
        if (addKey == true)
        {
            UpdateType = "add";
        }
        else
        {
            UpdateType = "remove";
        }
    }
}