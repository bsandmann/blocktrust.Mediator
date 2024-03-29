﻿namespace Blocktrust.Mediator.Common.Models.ProblemReport;

using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using FluentResults;
using Pickup;
using Protocols;

public class ProblemReport
{
    [JsonPropertyName("pc")] public ProblemCode ProblemCode { get;  set; }
    [JsonPropertyName("thid")] public string ThreadIdWhichCausedTheProblem { get;  set; }
    [JsonPropertyName("c")] public string? Comment { get;  set; }
    [JsonPropertyName("e")] public Uri? EscalateTo { get; set; }

    [JsonConstructor]
    public ProblemReport()
    {
        
    }


    public ProblemReport(ProblemCode problemCode, string threadIdWhichCausedTheProblem, string? comment = null, Uri? escalateTo = null)
    {
        ProblemCode = problemCode;
        ThreadIdWhichCausedTheProblem = threadIdWhichCausedTheProblem;
        Comment = comment;
        EscalateTo = escalateTo;
    }

    //Comparision for equals for ProblemCode, ThreadIdWhichCausedTheProblem, Comment and EscalateTo
    public override bool Equals(object obj)
    {
        if (obj is ProblemReport problemReport)
        {
            return this.ProblemCode.Equals(problemReport.ProblemCode) &&
                   this.ThreadIdWhichCausedTheProblem.Equals(problemReport.ThreadIdWhichCausedTheProblem) &&
                   (this.Comment ?? String.Empty).Equals(problemReport.Comment ?? string.Empty) &&
                   (this.EscalateTo is null ? string.Empty : this.EscalateTo.AbsolutePath).Equals(problemReport.EscalateTo is null ? string.Empty : problemReport.EscalateTo.AbsolutePath);
        }

        return false;
    }

    public static Result<ProblemReport> Parse(string problemCode, string threadIdWhichCausedTheProblem, string? comment = null, List<string>? commentArguments = null, Uri? escalateTo = null)
    {
        string? tmpComment = null;
        Uri? tmpEscalateTo = null;
        var problemCodeResult = ProblemCode.Parse(problemCode);
        if (problemCodeResult.IsFailed)
        {
            return problemCodeResult.ToResult();
        }

        var tmpProblemCode = problemCodeResult.Value;
        if (commentArguments is null || !commentArguments.Any())
        {
            tmpComment = comment;
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

                tmpComment = commentParsed;
            }
        }

        if (escalateTo is not null)
        {
            tmpEscalateTo = escalateTo;
        }

        return Result.Ok(new ProblemReport(tmpProblemCode, threadIdWhichCausedTheProblem, tmpComment, tmpEscalateTo));
    }

    public static Result<ProblemReport> Parse(DeliveryResponseModel responseModel)
    {
        var message = responseModel.Message;
        var messageId = responseModel.MessageId;

        if (message is null)
        {
            return Result.Fail("Message should not be null");
        }

        if (message.Type.Equals(ProtocolConstants.ProblemReport, StringComparison.InvariantCultureIgnoreCase))
        {
            return Result.Fail(string.Concat("Message is not a problem report. Tpye is '", message.Type, "'"));
        }

        if (string.IsNullOrEmpty(messageId))
        {
            return Result.Fail("MessageId should not be emtpy");
        }

        return Parse(message.Body, messageId);
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