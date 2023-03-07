namespace Blocktrust.Mediator.Server.Commands.DatabaseCommands.DeleteMessages;

using FluentResults;
using MediatR;
using Microsoft.EntityFrameworkCore;

public class DeleteMessagesHandler : IRequestHandler<DeleteMessagesRequest, Result>
{
    private readonly DataContext _context;


    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="context"></param>
    public DeleteMessagesHandler(DataContext context)
    {
        this._context = context;
    }

    /// <inheritdoc />
    public async Task<Result> Handle(DeleteMessagesRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var messages = await _context.StoredMessages
                .Where(p => request.MessageIds.Contains(p.MessageId) && p.RegisteredRecipient.MediatorConnection.RemoteDid.Equals(request.RemoteDid) && p.RegisteredRecipient.MediatorConnection.MediatorDid.Equals(request.MediatorDid))
                .ToListAsync(cancellationToken: cancellationToken);

            _context.StoredMessages.RemoveRange(messages);
            await _context.SaveChangesAsync(cancellationToken);

            return Result.Ok();
        }
        catch (Exception e)
        {
            return Result.Fail(e.Message);
        }
    }
}