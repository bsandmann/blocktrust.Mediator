namespace Blocktrust.Mediator.Server.Pages;

using Commands.DatabaseCommands.CreateOobInvitation;
using Commands.DatabaseCommands.CreateShortenedUrl;
using Commands.DatabaseCommands.GetOobInvitation;
using Common.Commands.CreatePeerDid;
using Common.Models.ShortenUrl;
using MediatR;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Net.Codecrete.QrCodeGenerator;
using PageModels;

public class BasePageModel : PageModel
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IMediator _mediator;

    public BasePageModel(IMediator mediator, IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
        _mediator = mediator;
    }

    public QRCodeModel QrCodeModel { get; set; }

    public async Task OnGet()
    {
        // This is a hack to show just the url when the user asking for the ?_oob= parameter
        if (this.Request.QueryString.HasValue && this.Request.QueryString.Value.Contains("?_oob="))
        {
            QrCodeModel = new QRCodeModel()
            {
                OobUrl = string.Concat(this.Request.Scheme, "://", this.Request.Host, this.Request.Path, this.Request.QueryString),
                OnlyDisplayUrl = true
            };
        }
        else
        {
            // This is essentially the same code as for the API
            var hostUrl = string.Concat(_httpContextAccessor!.HttpContext.Request.Scheme, "://", _httpContextAccessor.HttpContext.Request.Host);
            var existingInvitationResult = await _mediator.Send(new GetOobInvitationRequest(hostUrl));
            var invitation = string.Empty;
            if (existingInvitationResult.IsFailed)
            {
                var peerDidResponse = await _mediator.Send(new CreatePeerDidRequest(numberOfAgreementKeys: 1, numberOfAuthenticationKeys: 1, serviceEndpoint: new Uri(hostUrl), serviceRoutingKeys: new List<string>()));
                if (peerDidResponse.IsFailed)
                {
                    QrCodeModel = new QRCodeModel()
                    {
                        ErrorMessage = "We encountered an error creating a PeerDid on the fly. Please try again"
                    };
                }

                var result = await _mediator.Send(new CreateOobInvitationRequest(hostUrl, peerDidResponse.Value.PeerDid));
                if (result.IsFailed)
                {
                    QrCodeModel = new QRCodeModel()
                    {
                        ErrorMessage = "We encountered an error creating a the Out-Of-Band invitation on the fly. Please try again"
                    };
                }

                invitation = result.Value.Invitation;
            }
            else
            {
                invitation = existingInvitationResult.Value.Invitation;
            }

            var invitationUrl = string.Concat(hostUrl, "?_oob=", invitation);
            var qr = QrCode.EncodeText(invitationUrl, QrCode.Ecc.Medium);
            string svg = qr.ToSvgString(4);

            var createShortenedUrlResult = await _mediator.Send(new CreateShortenedUrlRequest(new Uri(invitationUrl), null, EnumShortenUrlGoalCode.ShortenOOBv2));
            if (createShortenedUrlResult.IsFailed)
            {
                QrCodeModel = new QRCodeModel()
                {
                    ErrorMessage = "We encountered an error creating a small QR-Code for Out-Of-Band invitation on the fly. Please try again"
                };
            }

            var shortUrl = string.Concat(hostUrl, createShortenedUrlResult.Value);
            var qrSmall = QrCode.EncodeText(shortUrl, QrCode.Ecc.Medium);
            string svgSmall = qrSmall.ToSvgString(4);

            QrCodeModel = new QRCodeModel()
            {
                OobUrl = invitationUrl,
                OobUrlShort = shortUrl,
                OobSvg = svg,
                OobSvgSmall = svgSmall,
                OnlyDisplayUrl = false
            };
        }
    }
}