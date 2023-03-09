namespace Blocktrust.Mediator.Server.Pages.PageModels;

public class QRCodeModel
{
    public string OobUrl { get; set; }
    public string OobUrlShort { get; set; }
    public string OobSvg { get; set; }
    public string OobSvgSmall { get; set; }
    public string ErrorMessage { get; set; }
}