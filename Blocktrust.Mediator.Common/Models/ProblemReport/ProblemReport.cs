namespace Blocktrust.Mediator.Common.Models.ProblemReport;

using System.Text.Json;
using FluentResults;

public class ProblemReport
{
    public ProblemCode ProblemCode { get; set; }
    public string ThreadIdWhichCausedTheProblem { get; set; }
    public string? Comment { get; set; }
    public Uri? EscalateTo { get; set; }


    public static Result<ProblemReport> Parse(string problemCode, string threadIdWhichCausedTheProblem, string? comment = null, List<string>? commentArguments = null, Uri? escalateTo = null)
    {
        var problemReport = new ProblemReport();
        var problemCodeResult = ProblemCode.Parse(problemCode);
        if (problemCodeResult.IsFailed)
        {
            return problemCodeResult.ToResult();
        }

        problemReport.ProblemCode = problemCodeResult.Value;
        problemReport.ThreadIdWhichCausedTheProblem = threadIdWhichCausedTheProblem;
        if (commentArguments is null || !commentArguments.Any())
        {
            problemReport.Comment = comment;
        }
        else
        {
            if (!string.IsNullOrEmpty(comment))
            {
                var commentParsed = comment;
                for (int i = 1; i < commentArguments.Count; i++)
                {
                    commentParsed = commentParsed.Replace("{" + i + "}", commentArguments[i]);
                }

                problemReport.Comment = commentParsed;
            }
        }

        if (escalateTo is not null)
        {
            problemReport.EscalateTo = escalateTo;
        }

        return Result.Ok(problemReport);
    }

    public static Result<ProblemReport> Parse(Dictionary<string, object?> body, string threadIdWhichCausedTheProblem)
    {
        string code;
        string? comment = null;
        List<string>? commentArguments = null;
        Uri? escalateTo = null;
        if (body.ContainsKey("code"))
        {
            body.TryGetValue("code", out var codeJson);
            var codeJsonElement = (JsonElement)codeJson!;
            if (codeJsonElement.ValueKind is JsonValueKind.String)
            {
                code = codeJsonElement.GetString()!;
            }
            else
            {
                return Result.Fail("Required field 'code' is invalid");
            }
        }
        else
        {
            return Result.Fail("Required field 'code' is missing");
        }

        if (body.ContainsKey("comment"))
        {
            body.TryGetValue("comment", out var commentJson);
            var commentJsonElement = (JsonElement)commentJson!;
            if (commentJsonElement.ValueKind is JsonValueKind.String)
            {
                comment = commentJsonElement.GetString();
            }
            else if (commentJsonElement.ValueKind is not JsonValueKind.Null)
            {
                return Result.Fail("Required field 'comment' is invalid");
            }
        }

        if (body.ContainsKey("args"))
        {
            body.TryGetValue("args", out var commentArgumentsJson);
            var commentArgumentsJsonElement = (JsonElement)commentArgumentsJson!;
            if (commentArgumentsJsonElement.ValueKind is JsonValueKind.Array)
            {
                foreach (var argument in commentArgumentsJsonElement.EnumerateArray())
                {
                    if (argument.ValueKind is JsonValueKind.String)
                    {
                        if (commentArguments is null)
                        {
                            commentArguments = new List<string>();
                        }

                        commentArguments.Add(argument.GetString()!);
                    }
                }
            }
            else if (commentArgumentsJsonElement.ValueKind is not JsonValueKind.Null)
            {
                return Result.Fail("Required field 'args' is invalid");
            }
        }

        if (body.ContainsKey("escalate_to"))
        {
            body.TryGetValue("escalate_to", out var escalateToJson);
            var escalateToJsonElement = (JsonElement)escalateToJson!;
            if (escalateToJsonElement.ValueKind is JsonValueKind.String)
            {
                var isParsed = Uri.TryCreate(escalateToJsonElement.GetString(), UriKind.Absolute, out escalateTo);
                if (!isParsed || escalateTo is null)
                {
                    return Result.Fail("Required field 'escalate_to' is invalid");
                }
            }
            else if (escalateToJsonElement.ValueKind is not JsonValueKind.Null)
            {
                return Result.Fail("Required field 'escalate_to' is invalid");
            }
        }

        return Parse(code, threadIdWhichCausedTheProblem, comment, commentArguments, escalateTo);
    }
}