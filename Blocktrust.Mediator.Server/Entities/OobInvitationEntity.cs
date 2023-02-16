namespace Blocktrust.Mediator.Server.Entities;

public class OobInvitationEntity
{
    /// <summary>
    /// The Id as Guid
    /// </summary>
    public Guid OobId { get; set; }

    /// <summary>
    /// The creation date of this entry
    /// </summary>
    public DateTime CreatedUtc { get; set; }

    /// <summary>
    /// The DID of this mediator created for the OOB-Invitation
    /// </summary>
    public string Did { get; set; }

    /// <summary>
    /// The URL this mediator the invitation was created for
    /// </summary>
    public string Url { get; set; }

    /// <summary>
    /// The Base64 encoded invitation
    /// </summary>
    public string Invitation { get; set; }
}