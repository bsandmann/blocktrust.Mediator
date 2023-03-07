namespace Blocktrust.Mediator.Server.Commands.DatabaseCommands.GetRegisteredRecipients;

using Blocktrust.Mediator.Server.Models;
using FluentResults;
using MediatR;
using Microsoft.EntityFrameworkCore;

public class GetRegisteredRecipientsHandler : IRequestHandler<GetRegisteredRecipientsRequest, Result<List<KeyEntryModel>>>
{
    private readonly DataContext _context;


    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="context"></param>
    public GetRegisteredRecipientsHandler(DataContext context)
    {
        this._context = context;
    }

    public async Task<Result<List<KeyEntryModel>>> Handle(GetRegisteredRecipientsRequest request, CancellationToken cancellationToken)
    {
        try
        {

            var connection = await _context.MediatorConnections.Include(p => p.RegisteredRecipients).FirstOrDefaultAsync(p => p.RemoteDid.Equals(request.RemoteDid) && p.MediationGranted, cancellationToken: cancellationToken);
            if (connection is not null)
            {
                return Result.Ok(connection.RegisteredRecipients.Select(p => new KeyEntryModel()
                {
                    KeyEntry = p.RecipientDid,
                    RemoteDid = connection.RemoteDid
                }).ToList());
            }

            return Result.Fail("Connection not found or not registered as mediator");
        }
        catch (Exception e)
        {
            return Result.Fail(e.Message);
        }
    }
}