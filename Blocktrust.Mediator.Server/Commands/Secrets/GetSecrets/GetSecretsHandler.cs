namespace Blocktrust.Mediator.Server.Commands.Secrets.GetSecrets;

using Blocktrust.Common.Models.DidDoc;
using Blocktrust.Common.Models.Secrets;
using FluentResults;
using MediatR;
using Microsoft.EntityFrameworkCore;

public class GetSecretsHandler : IRequestHandler<GetSecretsRequest, Result<List<Secret>>>
{
    private readonly DataContext _context;


    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="context"></param>
    public GetSecretsHandler(DataContext context)
    {
        this._context = context;
    }

    /// <summary>
    /// Handler
    /// </summary>
    /// <param name="saveSecretsRequest"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<Result<List<Secret>>> Handle(GetSecretsRequest saveSecretsRequest, CancellationToken cancellationToken)
    {
        var secretEntities = await _context.Secrets.Where(p => saveSecretsRequest.Kids.Contains(p.Kid)).ToListAsync(cancellationToken: cancellationToken);

        return Result.Ok(secretEntities.Select(p => new Secret(
            kid: p.Kid,
            type: (VerificationMethodType)p.VerificationMethodType,
            verificationMaterial: new VerificationMaterial(
                format: (VerificationMaterialFormat)p.VerificationMaterialFormat,
                value: p.Value
            )
        )).ToList());
    }
}