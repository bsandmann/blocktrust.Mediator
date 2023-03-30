namespace Blocktrust.Mediator.Server.Commands.ShortenedUrl.ProcessInvalidateShortenedUrl;

using System.Text.Json;
using Blocktrust.DIDComm.Message.Messages;
using Blocktrust.Mediator.Common.Models.ProblemReport;
using DatabaseCommands.InvalidateShortenedUrl;
using MediatR;

public class ProcessInvalidateShortenedUrlHandler : IRequestHandler<ProcessInvalidateShortenedUrlRequest, Message?>
{
    private readonly IMediator _mediator;
    private readonly IHttpContextAccessor _httpContextAccessor;

    /// <summary>
    /// Constructor
    /// </summary>
    public ProcessInvalidateShortenedUrlHandler(IMediator mediator, IHttpContextAccessor httpContextAccessor)
    {
        this._mediator = mediator;
        this._httpContextAccessor = httpContextAccessor;
    }

    /// <inheritdoc />
    public async Task<Message?> Handle(ProcessInvalidateShortenedUrlRequest request, CancellationToken cancellationToken)
    {
        var body = request.UnpackedMessage.Body;
        string? shortenedUrl;
        var hasShortenedUrl = body.TryGetValue("shortened_url", out var shortenedUrlJson);
        if (hasShortenedUrl)
        {
            var shortenedUrlJsonElement = (JsonElement)shortenedUrlJson!;
            if (shortenedUrlJsonElement.ValueKind is JsonValueKind.String)
            {
                shortenedUrl = shortenedUrlJsonElement.GetString();
            }
            else
            {
                return ProblemReportMessage.BuildDefaultMessageMissingArguments(
                    errorMessage: "Invalid body format: shortened_url",
                    threadIdWhichCausedTheProblem: request.UnpackedMessage.Thid ?? request.UnpackedMessage.Id,
                    fromPrior: request.FromPrior);
            }
        }
        else
        {
            return ProblemReportMessage.BuildDefaultMessageMissingArguments(
                errorMessage: "Missing 'shortened_url' in body",
                threadIdWhichCausedTheProblem: request.UnpackedMessage.Thid ?? request.UnpackedMessage.Id,
                fromPrior: request.FromPrior);
        }


        var splitted = shortenedUrl!.Split("?_oobid=");
        if (splitted.Length < 2 || splitted.Length > 3)
        {
            return ProblemReportMessage.BuildDefaultMessageMissingArguments(
                errorMessage: "Invalid body format: shortened_url",
                threadIdWhichCausedTheProblem: request.UnpackedMessage.Thid ?? request.UnpackedMessage.Id,
                fromPrior: request.FromPrior);
        }

        var guid = splitted[1];
        var parseable = Guid.TryParse(guid, out var shortenedUrlEntityId);
        if (!parseable)
        {
            return ProblemReportMessage.BuildDefaultMessageMissingArguments(
                errorMessage: "Invalid body format: shortened_url. Invalid id",
                threadIdWhichCausedTheProblem: request.UnpackedMessage.Thid ?? request.UnpackedMessage.Id,
                fromPrior: request.FromPrior);
        }

        var createShortenedUrlResult = await _mediator.Send(new InvalidateShortenedUrlRequest(shortenedUrlEntityId), cancellationToken);
        if (createShortenedUrlResult.IsFailed)
        {
            return ProblemReportMessage.BuildDefaultInternalError(
                errorMessage: createShortenedUrlResult.Errors.FirstOrDefault().Message,
                threadIdWhichCausedTheProblem: request.UnpackedMessage.Thid ?? request.UnpackedMessage.Id,
                fromPrior: request.FromPrior);
        }

        return null;
    }
}