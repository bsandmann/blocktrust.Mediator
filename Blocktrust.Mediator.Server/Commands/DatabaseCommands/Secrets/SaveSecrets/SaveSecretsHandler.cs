namespace Blocktrust.Mediator.Server.Commands.DatabaseCommands.Secrets.SaveSecrets;

using Blocktrust.Mediator.Server.Entities;
using FluentResults;
using MediatR;

public class SaveSecretsHandler : IRequestHandler<SaveSecretRequest, Result>
{
    private readonly DataContext _context;


    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="context"></param>
    public SaveSecretsHandler(DataContext context)
    {
        this._context = context;
    }

    /// <summary>
    /// Handler
    /// </summary>
    /// <param name="saveSecretRequest"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<Result> Handle(SaveSecretRequest saveSecretRequest, CancellationToken cancellationToken)
    {
        var secretEntity = new SecretEntity()
        {
            CreatedUtc = DateTime.UtcNow,
            Kid = saveSecretRequest.Kid,
            Value = saveSecretRequest.Secret.VerificationMaterial.Value,
            VerificationMaterialFormat = (int)saveSecretRequest.Secret.VerificationMaterial.Format,
            VerificationMethodType = (int)saveSecretRequest.Secret.Type
        };
        await _context.AddAsync(secretEntity, cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }
}