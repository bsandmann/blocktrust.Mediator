using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Blocktrust.Mediator.Server.Pages;

using Commands.OutOfBand.CreateOobInvitation;
using Commands.OutOfBand.GetOobInvitation;
using Common.Commands.CreatePeerDid;
using MediatR;
using Net.Codecrete.QrCodeGenerator;
using PageModels;

public class Index : PageModel
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IMediator _mediator;

    public Index(IMediator mediator, IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
        _mediator = mediator;
    }

    public IndexPageModel IndexPageModel { get; set; }

    public async Task OnGet()
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
                IndexPageModel = new IndexPageModel()
                {
                    ErrorMessage = "We encountered an error creating a PeerDid on the fly. Please try again"
                };
            }

            var result = await _mediator.Send(new CreateOobInvitationRequest(hostUrl, peerDidResponse.Value.PeerDid));
            if (result.IsFailed)
            {
                IndexPageModel = new IndexPageModel()
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

        IndexPageModel = new IndexPageModel()
        {
            OobUrl = invitationUrl,
            OobSvg = svg
        };
    }
}