namespace Blocktrust.Mediator.Common.Models.DiscoverFeatures;

using System.Text.Json;
using System.Text.Json.Serialization;
using FluentResults;

public class DiscoverFeature
{
    [JsonPropertyName("feature-type")] public string FeatureType { get; set; }

    [JsonPropertyName("id")] public string Id { get; set; }

    [JsonPropertyName("roles")] public List<string> Roles { get; set; }

    public DiscoverFeature(string featureType, string id, List<string>? roles = null)
    {
        FeatureType = featureType;
        Id = id;
        if (roles == null)
        {
            Roles = new List<string>();
        }
        else
        {
            Roles = roles;
        }
    }

    public static Result<List<DiscoverFeature>> Parse(Dictionary<string, object?> body)
    {
        if (body.ContainsKey("disclosures"))
        {
            var hasKeys = body.TryGetValue("disclosures", out var disclosures);
            if (hasKeys)
            {
                var disclosuresAsJsonElement = (JsonElement)disclosures!;
                var returnList = new List<DiscoverFeature>();
                if (disclosuresAsJsonElement.ValueKind == JsonValueKind.Array)
                {
                    foreach (var entry in disclosuresAsJsonElement.EnumerateArray())
                    {
                        string? featureType = null;
                        string? id = null;
                        var hasFeatureType = entry.TryGetProperty("feature-type", out var featureTypeJsonElement);
                        if (hasFeatureType && featureTypeJsonElement.ValueKind == JsonValueKind.String)
                        {
                            featureType = featureTypeJsonElement.GetString();
                        }

                        var hasId = entry.TryGetProperty("id", out var idJsonElement);
                        if (hasId && idJsonElement.ValueKind == JsonValueKind.String)
                        {
                            id = idJsonElement.GetString();
                        }

                        var hasRoles = entry.TryGetProperty("roles", out var rolesJsonElement);
                        var roles = new List<string>();
                        if (hasRoles && rolesJsonElement.ValueKind == JsonValueKind.Array)
                        {
                            foreach (var roleJsonElement in rolesJsonElement.EnumerateArray())
                            {
                                if (roleJsonElement.ValueKind == JsonValueKind.String)
                                {
                                    roles.Add(roleJsonElement.GetString()!);
                                }
                            }
                        }

                        if (!string.IsNullOrEmpty(featureType) && !string.IsNullOrEmpty(id))
                        {
                            returnList.Add(new DiscoverFeature(featureType, id, roles));
                        }
                    }

                    return Result.Ok(returnList);
                }
            }
        }

        return Result.Fail("Unexpected body. Missing 'disclosures' key");
    }
}