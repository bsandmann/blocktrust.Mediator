namespace Blocktrust.Mediator.Common.Models.Credential;


public class InnerCredential
{
    public string Subject { get; set; }
    public Dictionary<string,string> Claims { get; set; }
    public List<string> Type { get; set; }
    public List<string> Context { get; set; }
}