namespace Blocktrust.Mediator.Server.Pages;

using MediatR;

public class OobQRCodeSmall : BasePageModel
{
    public OobQRCodeSmall(IMediator mediator, IHttpContextAccessor httpContextAccessor) : base(mediator,httpContextAccessor)
    {
    }
    
}