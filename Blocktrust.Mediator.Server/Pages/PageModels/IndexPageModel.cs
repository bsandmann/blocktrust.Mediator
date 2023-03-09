namespace Blocktrust.Mediator.Server.Pages.PageModels;

using System.ComponentModel.DataAnnotations;

public class IndexPageModel
{
    public string OobUrl { get; set; }
    public string OobSvg { get; set; }
    public string ErrorMessage { get; set; }
}