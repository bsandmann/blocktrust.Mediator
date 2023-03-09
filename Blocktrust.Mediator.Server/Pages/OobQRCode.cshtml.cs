namespace Blocktrust.Mediator.Server.Pages;

using MediatR;

public class OobQRCode : BasePageModel
{
    public OobQRCode(IMediator mediator, IHttpContextAccessor httpContextAccessor) : base(mediator,httpContextAccessor)
    {
    }
    
}