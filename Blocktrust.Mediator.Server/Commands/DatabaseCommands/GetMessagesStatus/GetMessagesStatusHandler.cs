namespace Blocktrust.Mediator.Server.Commands.DatabaseCommands.GetMessagesStatus;

using Entities;
using FluentResults;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Models;

public class GetMessagesStatusHandler : IRequestHandler<GetMessagesStatusRequest, Result<MessagesStatusModel>>
{
    private readonly DataContext _context;


    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="context"></param>
    public GetMessagesStatusHandler(DataContext context)
    {
        this._context = context;
    }

    public async Task<Result<MessagesStatusModel>> Handle(GetMessagesStatusRequest request, CancellationToken cancellationToken)
    {
        //TODO this all can be done more efficient
        //also recipientKEy vs recipientDid
        try
        {
            var connection = await _context.MediatorConnections
                .Include(p => p.RegisteredRecipients)
                .FirstOrDefaultAsync(p => p.RemoteDid.Equals(request.RemoteDid) && p.MediatorDid.Equals(request.MediatorDid), cancellationToken: cancellationToken);
            if (connection is null)
            {
                return Result.Fail("Connection was not found");
            }

            if (request.RecipientDid is null)
            {
                var allRecipientDidKeys = connection.RegisteredRecipients.Select(p => p.RecipientDid);
                // TODO highly unefficient! I just need the metatdata and not the messages itself
                var messages = await _context.StoredMessages.Where(p => allRecipientDidKeys.Contains(p.RegisteredRecipient.RecipientDid)).ToListAsync(cancellationToken: cancellationToken);

                return Result.Ok(new MessagesStatusModel(
                    messageCount: messages.Count,
                    recipientDid: null,
                    longestWaitedSeconds: (long)(DateTime.UtcNow - messages.MinBy(p => p.Created).Created).TotalSeconds,
                    newestMessageTime: (new DateTimeOffset(messages.MaxBy(p=>p.Created).Created)).ToUnixTimeMilliseconds(),
                    oldestMessageTime: (new DateTimeOffset(messages.MinBy(p=>p.Created).Created)).ToUnixTimeMilliseconds(),
                    totalByteSize: messages.Sum(p=>p.MessageSize)));
            }
            else
            {
                var selectedRecipientDidKey = connection.RegisteredRecipients.FirstOrDefault(p => p.RecipientDid.Equals(request.RecipientDid));
                if (selectedRecipientDidKey is null)
                {
                    return Result.Ok(new MessagesStatusModel(
                        messageCount: 0,
                        recipientDid: null,
                        longestWaitedSeconds: null,
                        newestMessageTime: null,
                        oldestMessageTime: null,
                        totalByteSize: null));
                }

                var messages = await _context.StoredMessages.Where(p => p.RegisteredRecipient.RecipientDid.Equals(selectedRecipientDidKey.RecipientDid)).ToListAsync(cancellationToken: cancellationToken);
                return Result.Ok(new MessagesStatusModel(
                    messageCount: messages.Count,
                    recipientDid: selectedRecipientDidKey.RecipientDid,
                    longestWaitedSeconds: (long)(DateTime.UtcNow - messages.MinBy(p => p.Created).Created).TotalSeconds,
                    newestMessageTime: (new DateTimeOffset(messages.MaxBy(p=>p.Created).Created)).ToUnixTimeMilliseconds(),
                    oldestMessageTime: (new DateTimeOffset(messages.MinBy(p=>p.Created).Created)).ToUnixTimeMilliseconds(),
                    totalByteSize: messages.Sum(p=>p.MessageSize)));
            }
        }
        catch (Exception e)
        {
            return Result.Fail(e.Message);
        }
    }
}