namespace Blocktrust.Mediator.Common.Models.ProblemReport;

using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Blocktrust.Common.Converter;
using DIDComm.Message.Messages;
using DIDComm.Operations;
using Protocols;
using FromPrior = DIDComm.Message.FromPriors.FromPrior;

public static class ProblemReportMessage
{
    public static Message Build(FromPrior? fromPrior, string threadIdWhichCausedTheProblem, ProblemCode problemCode, string? comment, List<string>? commentArguments, Uri? escalateTo)
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
            .pthid(threadIdWhichCausedTheProblem)
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