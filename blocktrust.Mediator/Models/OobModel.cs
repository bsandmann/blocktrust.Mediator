namespace Blocktrust.Mediator.Models;

using Entities;

public class OobModel
{
    public Guid OobId { get; }

    public DateTime CreatedUtc { get; }

    public string Did { get; }

    public Uri Url { get; }

    public OobModel(OobEntity oobEntity)
    {
        OobId = oobEntity.OobId;
        CreatedUtc = oobEntity.CreatedUtc;
        Did = oobEntity.Did;
        Url = new Uri(oobEntity.Url);
    }
}