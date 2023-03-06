namespace Blocktrust.Mediator.Server.Commands.MediatorCoordinator.ProcessQueryMediatorKeys;

using System.Text.Json;
using Blocktrust.DIDComm.Message.Messages;
using Blocktrust.Mediator.Common.Models.MediatorCoordinator;
using Blocktrust.Mediator.Common.Protocols;
using DatabaseCommands.GetConnection;
using DatabaseCommands.GetKeyEntries;
using DatabaseCommands.StoreMessage;
using DIDComm.Message.Attachments;
using DIDComm.Utils;
using FluentResults;
using ForwardMessage;
using MediatR;
using Models;
using Org.BouncyCastle.Crypto.Parameters;

public class ProcessForwardMessageHandler : IRequestHandler<ProcessForwardMessageRequest, Result>
{
    private readonly IMediator _mediator;

    /// <summary>
    /// Constructor
    /// </summary>
    public ProcessForwardMessageHandler(IMediator mediator)
    {
        this._mediator = mediator;
    }

    //TODO check everywhere: recipient_did vs recipient_key

    // TODO dig into https://identity.foundation/didcomm-messaging/spec/#routing-protocol-20

    public async Task<Result> Handle(ProcessForwardMessageRequest request, CancellationToken cancellationToken)
    {
        var existingConnection = await _mediator.Send(new GetConnectionRequest(request.SenderDid), cancellationToken);
        if (existingConnection.IsFailed)
        {
            // database error
        }

        if (existingConnection.Value is null && !existingConnection.Value!.MediationGranted)
        {
            return Result.Fail("Connection does not exist or mediation is not granted");
        }
        else
        {
            var body = request.UnpackedMessage.Body;
            var hasNext = body.TryGetValue("next", out var next);
            if (!hasNext)
            {
                return Result.Fail("Invalid body");
            }

            var nextJsonElement = (JsonElement)next;
            var recipientDid = nextJsonElement.GetString();
            
            //TODO check if recipient is a valid DID (a single DID, not multiple DIDs)?
            if (string.IsNullOrEmpty(recipientDid))
            {
                return Result.Fail("Invalid body: recipient did is empty");
            }

            // TODO Possible code duplication with the DeliveryRequestHandler
            var attachments = request.UnpackedMessage.Attachments;
            string innerMessage = String.Empty;
            var messages = new List<StoredMessage>();
            foreach (var attachment in attachments)
            {
                var id = attachment.Id;
                var data = attachment.Data;
                if (data is Json)
                {
                    Json? jsonAttachmentData = (Json)data;
                    var innerJson = jsonAttachmentData?.JsonString;
                    var msg = innerJson?.GetTyped<Dictionary<string, object>>("json");
                    innerMessage = JsonSerializer.Serialize(msg);
                    messages.Add(new StoredMessage(id, innerMessage));
                }
                else
                {
                    throw new NotImplementedException("Not implemented yet");
                }
            }

            var storeMessageResult = await _mediator.Send(new StoreMessagesRequest(request.MediatorDid, recipientDid, messages), cancellationToken);
            if (storeMessageResult.IsFailed)
            {
                return storeMessageResult;
            }
            
            return Result.Ok();
        }
    }
}