namespace Blocktrust.Mediator.Server.Commands.DatabaseCommands.StoreMessage;

using Entities;
using FluentResults;
using MediatR;
using Microsoft.EntityFrameworkCore;

public class StoreMessageHandler : IRequestHandler<StoreMessagesRequest, Result>
{
    private readonly DataContext _context;


    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="context"></param>
    public StoreMessageHandler(DataContext context)
    {
        this._context = context;
    }

    public async Task<Result> Handle(StoreMessagesRequest request, CancellationToken cancellationToken)
    {
        _context.ChangeTracker.Clear();
        _context.ChangeTracker.AutoDetectChangesEnabled = false;
        //TODO this all can be done more efficient
        //also recipientKEy vs recipientDid
        try
        {
            var recipientKey = await _context.RegisteredRecipients.Include(p => p.StoredMessage).FirstOrDefaultAsync(p => p.RecipientDid.Equals(request.RecipientDid), cancellationToken: cancellationToken);
            if (recipientKey is null)
            {
                return Result.Fail("Recipient key not found");
            }

            var messages = new List<StoredMessage>();
            foreach (var message in request.Messages)
            {
                messages.Add(new StoredMessage()
                {
                    RecipientDid = recipientKey.RecipientDid,
                    Created = DateTime.Now,
                    MessageId = message.MessageId,
                    MessageHash = "123",
                    Message = message.Message,
                    MessageSize = System.Text.Encoding.UTF8.GetByteCount(message.Message)
                });
            }

            recipientKey.StoredMessage.AddRange(messages);
            _context.RegisteredRecipients.Update(recipientKey);
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (Exception e)
        {
            return Result.Fail(e.Message);
        }

        return Result.Ok();
    }
}