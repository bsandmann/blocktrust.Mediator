namespace Blocktrust.Mediator.Server.Utils;

using System.Text.Json;
using System.Text.Json.Serialization;

public class JsonUtils
{
    public static JsonSerializerOptions CommonSerializationOptions = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        // TODO The default serialization options encode the + sign as \u002B. This is not compatible with Kotlin tests and implementation
        // It can be considered as as security risk, but for now we will use the unsafe relaxed option
        // Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    }; 
}