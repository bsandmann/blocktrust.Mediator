namespace Blocktrust.Mediator.Server.Commands.DatabaseCommands.UpdateRegisteredRecipients;

using Blocktrust.Mediator.Server.Entities;
using FluentResults;
using MediatR;
using Microsoft.EntityFrameworkCore;

public class UpdateRegisteredRecipientsHandler : IRequestHandler<UpdateRegisteredRecipientsRequest, Result>
{
    private readonly DataContext _context;


    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="context"></param>
    public UpdateRegisteredRecipientsHandler(DataContext context)
    {
        this._context = context;
    }

    public async Task<Result> Handle(UpdateRegisteredRecipientsRequest request, CancellationToken cancellationToken)
    {
        try
        {
            _context.ChangeTracker.Clear();
            _context.ChangeTracker.AutoDetectChangesEnabled = false;

            var connection = await _context.MediatorConnections.Include(p => p.RegisteredRecipients).FirstOrDefaultAsync(p => p.RemoteDid.Equals(request.RemoteDid) && p.MediationGranted, cancellationToken: cancellationToken);
            if (connection is not null)
            {
                if (request.KeysToAdd.Any())
                {
                    var keysToAdd = new List<RegisteredRecipient>();
                    foreach (var keyToAdd in request.KeysToAdd)
                    {
                        if (!connection.RegisteredRecipients.Select(p => p.RecipientDid).Contains(keyToAdd))
                        {
                            keysToAdd.Add(
                                new RegisteredRecipient()
                                {
                                    RecipientDid = keyToAdd,
                                    MediatorConnectionId = connection.MediatorConnectionId
                                });
                        }
                    }

                    _context.RegisteredRecipients.AddRange(keysToAdd);
                    await _context.SaveChangesAsync(cancellationToken);
                }

                if (request.KeysToRemove.Any())
                {
                    foreach (var keyToRemove in request.KeysToRemove)
                    {
                        var keyEntityToRemove = connection.RegisteredRecipients.FirstOrDefault(p => p.RecipientDid.Equals(keyToRemove));
                        if (keyEntityToRemove is not null)
                        {
                            _context.Remove(keyEntityToRemove);
                        }
                    }

                    await _context.SaveChangesAsync(cancellationToken);
                }

                return Result.Ok();
            }

            return Result.Fail("Connection not found or not registered as mediator");
        }
        catch (Exception e)
        {
            return Result.Fail(e.Message);
        }
    }
}