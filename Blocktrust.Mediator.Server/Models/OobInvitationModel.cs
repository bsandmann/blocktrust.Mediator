namespace Blocktrust.Mediator.Server.Models;

using Entities;

public class OobInvitationModel
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

    public OobInvitationModel(OobInvitationEntity oobInvitationEntity)
    {
        OobId = oobInvitationEntity.OobId;
        CreatedUtc = oobInvitationEntity.CreatedUtc;
        Did = oobInvitationEntity.Did;
        Url = oobInvitationEntity.Url;
        Invitation = oobInvitationEntity.Invitation;
    }
}