namespace Blocktrust.Mediator.Server.Commands.Connections.GetKeyEntries;

using FluentResults;
using GetConnection;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Models;

public class GetKeyEntriesHandler : IRequestHandler<GetKeyEntriesRequest, Result<List<KeyEntryModel>>>
{
    private readonly DataContext _context;


    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="context"></param>
    public GetKeyEntriesHandler(DataContext context)
    {
        this._context = context;
    }

    public async Task<Result<List<KeyEntryModel>>> Handle(GetKeyEntriesRequest request, CancellationToken cancellationToken)
    {
        try
        {

            var connection = await _context.Connections.Include(p => p.KeyList).FirstOrDefaultAsync(p => p.RemoteDid.Equals(request.RemoteDid) && p.MediationGranted, cancellationToken: cancellationToken);
            if (connection is not null)
            {
                return Result.Ok(connection.KeyList.Select(p => new KeyEntryModel()
                {
                    KeyEntry = p.Key,
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