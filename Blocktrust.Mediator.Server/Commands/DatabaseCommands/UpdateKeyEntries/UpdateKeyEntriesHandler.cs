namespace Blocktrust.Mediator.Server.Commands.DatabaseCommands.UpdateKeyEntries;

using Blocktrust.Mediator.Server.Entities;
using FluentResults;
using MediatR;
using Microsoft.EntityFrameworkCore;

public class UpdateKeyEntriesHandler : IRequestHandler<UpdateKeyEntriesRequest, Result>
{
    private readonly DataContext _context;


    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="context"></param>
    public UpdateKeyEntriesHandler(DataContext context)
    {
        this._context = context;
    }

    public async Task<Result> Handle(UpdateKeyEntriesRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var connection = await _context.Connections.Include(p => p.KeyList).FirstOrDefaultAsync(p => p.RemoteDid.Equals(request.RemoteDid) && p.MediationGranted, cancellationToken: cancellationToken);
            if (connection is not null)
            {
                if (request.KeysToAdd.Any())
                {
                    foreach (var keyToAdd in request.KeysToAdd)
                    {
                        if (!connection.KeyList.Select(p => p.Key).Contains(keyToAdd))
                        {
                            connection.KeyList.Add(new MediatorConnectionKeyEntity()
                            {
                                Key = keyToAdd,
                            });
                        }
                    }

                    _context.Connections.Update(connection);
                    await _context.SaveChangesAsync(cancellationToken);
                }

                if (request.KeysToRemove.Any())
                {
                    foreach (var keyToRemove in request.KeysToRemove)
                    {
                        var keyEntityToRemove = connection.KeyList.FirstOrDefault(p => p.Key.Equals(keyToRemove));
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