﻿namespace Blocktrust.Mediator.Server.Commands.DatabaseCommands.GetMessages;

using Blocktrust.Mediator.Server.Models;
using FluentResults;
using MediatR;
using Microsoft.EntityFrameworkCore;

public class GetMessagesHandler : IRequestHandler<GetMessagesRequest, Result<List<StoredMessageModel>>>
{
    private readonly DataContext _context;


    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="context"></param>
    public GetMessagesHandler(DataContext context)
    {
        this._context = context;
    }

    public async Task<Result<List<StoredMessageModel>>> Handle(GetMessagesRequest request, CancellationToken cancellationToken)
    {
        try
        {
            //TODO this can be done more elegant in one query each
            var connection = await _context.MediatorConnections
                .Include(p => p.RegisteredRecipients)
                .FirstOrDefaultAsync(p => p.RemoteDid.Equals(request.RemoteDid) && p.MediatorDid.Equals(request.MediatorDid) && p.MediationGranted, cancellationToken: cancellationToken);
            if (request.RecipientDid is null)
            {
                if (connection is null)
                {
                    return Result.Fail("Connection was not found. Mediation might not have been granted.");
                }

                var messages = await _context.StoredMessages
                    .Where(p => connection.RegisteredRecipients
                        .Select(q => q.RecipientDid)
                        .Contains(p.RegisteredRecipient.RecipientDid))
                    .Select(p => new StoredMessageModel(p.MessageId, p.Message))
                    .ToListAsync(cancellationToken: cancellationToken);

                return Result.Ok(messages);
            }
            else
            {
                if (connection is null)
                {
                    return Result.Fail("Connection was not found, with the given recipientDid");
                }

                var messages = await _context.StoredMessages
                    .Where(p => p.RegisteredRecipient.RecipientDid.Equals(request.RecipientDid))
                    .Select(p => new StoredMessageModel(p.MessageId, p.Message))
                    .ToListAsync(cancellationToken: cancellationToken);
                return Result.Ok(messages);
            }
        }
        catch (Exception e)
        {
            return Result.Fail(e.Message);
        }
    }
}