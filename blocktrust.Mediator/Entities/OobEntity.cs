namespace Blocktrust.Mediator.Entities;

public class OobEntity
{
    public Guid OobId { get; set; }

    public DateTime CreatedUtc { get; set; }

    public string Did { get; set; }

    public string Url { get; set; }
}