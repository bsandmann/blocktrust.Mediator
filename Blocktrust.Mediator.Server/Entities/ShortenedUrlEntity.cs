namespace Blocktrust.Mediator.Server.Entities;

public class ShortenedUrlEntity
{
    ///NOTE: If this was just modelled for shorten oob-invitations, the
    /// Id should be the same as the Id of the invitation itself.
    /// The entites would also be connected by a FK. This is not the case
    /// in this naive implementation
    
    /// <summary>
    /// The Id of this entry and also the identifier for the shortened url
    /// </summary>
    public Guid ShortenedUrlEntityId { get; set; }
   
    /// <summary>
    /// The long form which should be shortened
    /// </summary>
    public string LongFormUrl { get; set; }
    
    /// <summary>
    /// Date of creationd
    /// </summary>
    public DateTime CreatedUtc { get; set; }
    
    /// <summary>
    /// Date of the optiona the shorten-url-expiration-date
    /// </summary>

    public DateTime? ExpirationUtc { get; set; }
    
    /// <summary>
    /// A part of a slug, which was requested by the user to be used in the url
    /// </summary>

    public string? RequestedPartialSlug { get; set; }
    
    /// <summary>
    /// The Goal-Code with which the shortenedUrl was initially created
    /// </summary>
    
    public string GoalCode { get; set; }
}