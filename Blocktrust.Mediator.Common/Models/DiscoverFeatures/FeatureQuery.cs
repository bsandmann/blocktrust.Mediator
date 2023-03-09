namespace Blocktrust.Mediator.Common.Models.DiscoverFeatures;

using System.Text.Json;
using System.Text.Json.Serialization;

public class FeatureQuery
{
    [JsonPropertyName("feature-type")] public string FeatureType { get; set; }

    [JsonPropertyName("match")] public string Match { get; set; }


    public FeatureQuery()
    {
    }

    public FeatureQuery(string featureType, string match = "*")
    {
        FeatureType = featureType;
        Match = match;
    }

    public static List<FeatureQuery> Parse(Dictionary<string, object?> body)
    {
        var returnList = new List<FeatureQuery>();
        if (body.ContainsKey("queries"))
        {
            var hasQueries = body.TryGetValue("queries", out var queriesJson);
            if (hasQueries)
            {
                var queriesAsJsonElement = (JsonElement)queriesJson!;
                if (queriesAsJsonElement.ValueKind == JsonValueKind.Array)
                {
                    foreach (var entry in queriesAsJsonElement.EnumerateArray())
                    {
                        string? featureType = null;
                        string? match = null;
                        var hasFeatureType = entry.TryGetProperty("feature-type", out var featureTypeJsonElement);
                        if (hasFeatureType && featureTypeJsonElement.ValueKind == JsonValueKind.String)
                        {
                            featureType = featureTypeJsonElement.GetString();
                        }

                        var hasMatch = entry.TryGetProperty("match", out var idJsonElement);
                        if (hasMatch && idJsonElement.ValueKind == JsonValueKind.String)
                        {
                            match = idJsonElement.GetString();
                        }

                        if (!string.IsNullOrEmpty(featureType) && !string.IsNullOrEmpty(match))
                        {
                            returnList.Add(new FeatureQuery(featureType, match));
                        }
                    }
                }
            }
        }

        return returnList;
    }
}