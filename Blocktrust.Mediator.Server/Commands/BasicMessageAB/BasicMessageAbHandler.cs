namespace Blocktrust.Mediator.Server.Commands.BasicMessageAB;

using System.Text.Json;
using Blocktrust.DIDComm.Message.Messages;
using Blocktrust.Mediator.Common.Protocols;
using Common.Models.ProblemReport;
using MediatR;

public class BasicMessageAbHandler : IRequestHandler<BasicMessageAbRequest, Message?>
{
    /// <inheritdoc />
    public async Task<Message?> Handle(BasicMessageAbRequest request, CancellationToken cancellationToken)
    {
        var body = request.UnpackedMessage.Body;
        string content = String.Empty;
        var hasContent = body.TryGetValue("content", out var contentBody);
        if (hasContent)
        {
            var contentJsonElement = (JsonElement)contentBody!;
            if (contentJsonElement.ValueKind is JsonValueKind.String)
            {
                content = contentJsonElement.GetString()??string.Empty;
            }
            else
            {
                return ProblemReportMessage.BuildDefaultMessageMissingArguments(
                    errorMessage: "Invalid body format: missing content",
                    threadIdWhichCausedTheProblem: request.UnpackedMessage.Thid ?? request.UnpackedMessage.Id,
                    fromPrior: request.FromPrior);
            }
        } 

        var returnBody = new Dictionary<string, object>();
        returnBody.Add("content", String.Concat("This is the BLOCKTRUST MEDIATOR answering machine. Thank you for calling! Your message was: '", content,"'"));
        var returnMessage = new MessageBuilder(
                id: Guid.NewGuid().ToString(),
                type: ProtocolConstants.BasicMessage,
                body: returnBody
            )
            .thid(request.UnpackedMessage.Thid ?? request.UnpackedMessage.Id)
            .fromPrior(request.FromPrior)
            .build();

        return returnMessage;
    }
}