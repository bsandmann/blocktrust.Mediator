namespace Blocktrust.Mediator.Common.Models.ProblemReport;

using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Blocktrust.Common.Converter;
using DIDComm.Message.Messages;
using DIDComm.Operations;
using Protocols;
using FromPrior = DIDComm.Message.FromPriors.FromPrior;

public class ProblemReportModel
{
    /// <summary>
    /// REQUIRED. The header conveying the DIDComm MTURI: "https://didcomm.org/report-problem/2.0/problem-report"
    /// </summary>
    [JsonPropertyName("type")]
    public string Type { get; set; }

    /// <summary> 
    ///  REQUIRED. 
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; }

    /// <summary> 
    ///  REQUIRED. The value is the thid of the thread in which the problem occurred.
    /// (Thus, the problem report begins a new child thread, of which the triggering context is the parent.
    /// The parent context can react immediately to the problem, or can suspend progress while
    /// troubleshooting occurs.)
    /// </summary>
    [JsonPropertyName("pthid")]
    public string ParentThreadId { get; set; }

    // TODO not used
    /// <summary> 
    /// OPTIONAL. It SHOULD be included if the problem in question was triggered directly by a preceding message.
    /// </summary>
    [JsonPropertyName("ack")]
    public List<string> Acknowledgements { get; set; }

    /// <summary> 
    /// REQUIRED 
    /// </summary>
    [JsonPropertyName("body")]
    public ProblemReportBodyModel Body { get; set; }


    public static Message BuildProblemReport(FromPrior? fromPrior, string threadIdWithCausedTheProblem, ProblemCode problemCode, string? comment, List<string>? commentArguments, Uri? escalateTo)
    {
        var body = new Dictionary<string, object>();
        body.Add("code", problemCode.ToString());
        if (comment is not null)
        {
            body.Add("comment", comment);
        }

        if (commentArguments is not null && commentArguments.Any())
        {
            body.Add("args", commentArguments);
        }

        if (escalateTo is not null)
        {
            body.Add("escalate_to", escalateTo);
        }

        var msg = new MessageBuilder(
                id: Guid.NewGuid().ToString(),
                type: ProtocolConstants.ProblemReport,
                body: body
            )
            .thid(threadIdWithCausedTheProblem)
            .fromPrior(fromPrior)
            .build();

        //TODO better understand acks and add them here
        // if (acknowledgements is not null && commentArguments.Any())
        // {
        //     msg.Acknowledgements = acknowledgements;
        // }

        return msg;
    }
}