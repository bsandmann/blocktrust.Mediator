namespace Blocktrust.Mediator.Server.Entities;

public class SecretEntity
{
    //TODO is a oversimplicfication, since i should save the kid then add multiple VerifactionMaterialEntties to that kid
    //Also I guess there will be multiple other entries related to that did (like also the invitation) or logs

    /// <summary>
    /// The Id as Guid
    /// </summary>
    public Guid SecretId { get; set; }

    /// <summary>
    /// The key-id-of the secret
    /// </summary>
    public string Kid { get; set; }

    /// <summary>
    /// The MethodType enum as int (see VerificationMethodType)
    /// </summary>
    public int VerificationMethodType { get; set; }

    /// <summary>
    /// The format enum as int (see VerificationMaterialFormat)
    /// </summary>
    public int VerificationMaterialFormat { get; set; }

    /// <summary>
    /// The value of the secret: Depending on the format this could be eg. a Base64 encoded string or a JWK 
    /// </summary>
    public string Value { get; set; }

    /// <summary>
    /// Date of creation
    /// </summary>
    public DateTime CreatedUtc { get; set; }
}