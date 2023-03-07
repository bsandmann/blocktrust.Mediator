namespace Blocktrust.Mediator.Common.Models.ProblemReport;

using System.Text.Json.Serialization;

public class ProblemReportBodyModel
{
    /// <summary> 
    /// REQUIRED. Encoded with the ProblemCode class
    /// </summary>
    [JsonPropertyName("id")]
    public string Code { get; set; }

    /// <summary> 
    /// OPTIONAL but recommended. Contains human-friendly text describing the problem.
    /// Usage example:  "Unable to use the {1} endpoint for {2}." {x} represent the arguments in the args field.
    /// </summary>
    [JsonPropertyName("comment")]
    public string Comment { get; set; }

    /// <summary> 
    /// OPTIONAL but recommended. Arguments to the human-friendly text describing the problem in the comment field.
    /// Usage example:  
    /// </summary>
    [JsonPropertyName("args")]
    public List<string> CommentArguments { get; set; }

    /// <summary> 
    /// OPTIONAL. Provides a URI where additional help on the issue can be received.
    /// eg. mailto:bug@blockrtust.dev
    /// </summary>
    [JsonPropertyName("escalate_to")]
    public Uri EscalateTo { get; set; }
}