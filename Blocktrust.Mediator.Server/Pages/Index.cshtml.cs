namespace Blocktrust.Mediator.Server.Pages;

using MediatR;

public class Index : BasePageModel
{
    public Index(IMediator mediator, IHttpContextAccessor httpContextAccessor) : base(mediator,httpContextAccessor)
    {
    }
    
}