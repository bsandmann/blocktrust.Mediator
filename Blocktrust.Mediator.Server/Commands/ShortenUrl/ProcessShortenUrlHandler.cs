namespace Blocktrust.Mediator.Server.Commands.ShortenUrl;

using Blocktrust.DIDComm.Message.Messages;
using Blocktrust.Mediator.Common.Protocols;
using Common.Models.ProblemReport;
using Common.Models.ShortenUrl;
using DatabaseCommands.CreateShortenedUrl;
using MediatR;

public class ProcessShortenUrlHandler : IRequestHandler<ProcessShortenUrlRequest, Message?>
{
    private readonly IMediator _mediator;
    private readonly IHttpContextAccessor _httpContextAccessor;

    /// <summary>
    /// Constructor
    /// </summary>
    public ProcessShortenUrlHandler(IMediator mediator, IHttpContextAccessor httpContextAccessor)
    {
        this._mediator = mediator;
        this._httpContextAccessor = httpContextAccessor;
    }

    /// <inheritdoc />
    public async Task<Message?> Handle(ProcessShortenUrlRequest request, CancellationToken cancellationToken)
    {
        var hostUrl = string.Concat(_httpContextAccessor!.HttpContext.Request.Scheme, "://", _httpContextAccessor.HttpContext.Request.Host);
        var shortenedUrlResult = ShortenedUrl.Parse(request.UnpackedMessage.Body);
        var createShortenedUrlResult = await _mediator.Send(new CreateShortenedUrlRequest(shortenedUrlResult.Value.UrlToShorten, shortenedUrlResult.Value.ShortUrlSlug, shortenedUrlResult.Value.GoalCode), cancellationToken);
        if (createShortenedUrlResult.IsFailed)
        {
            return ProblemReportMessage.BuildDefaultInternalError(
                errorMessage: createShortenedUrlResult.Errors.FirstOrDefault().Message,
                threadIdWhichCausedTheProblem: request.UnpackedMessage.Thid ?? request.UnpackedMessage.Id,
                fromPrior: request.FromPrior);
        }

        var returnBody = new Dictionary<string, object>();
        returnBody.Add("shortened_url", string.Concat(hostUrl, createShortenedUrlResult.Value));
        //TODO there should also be an expires times header. I leave that out for now, its optional
        var disclosureMessage = new MessageBuilder(
                id: Guid.NewGuid().ToString(),
                type: ProtocolConstants.ShortenedUrlResponse,
                body: returnBody
            )
            .thid(request.UnpackedMessage.Thid ?? request.UnpackedMessage.Id)
            .fromPrior(request.FromPrior)
            .build();

        return disclosureMessage;
    }
}