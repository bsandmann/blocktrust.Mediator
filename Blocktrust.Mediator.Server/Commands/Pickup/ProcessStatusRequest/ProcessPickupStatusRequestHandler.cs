namespace Blocktrust.Mediator.Server.Commands.MediatorCoordinator.ProcessMediationRequest;

using System.Text.Json;
using Blocktrust.DIDComm.Message.Messages;
using Blocktrust.Mediator.Common.Commands.CreatePeerDid;
using Blocktrust.Mediator.Common.Protocols;
using DatabaseCommands.GetConnection;
using DatabaseCommands.GetMessagesStatus;
using DatabaseCommands.UpdateConnection;
using FluentResults;
using MediatR;
using Pickup.ProcessStatusRequest;

public class ProcessPickupStatusRequestHandler : IRequestHandler<ProcessPickupStatusRequestRequest, Result<Message>>
{
    private readonly IMediator _mediator;

    /// <summary>
    /// Constructor
    /// </summary>
    public ProcessPickupStatusRequestHandler(IMediator mediator)
    {
        this._mediator = mediator;
    }

    public async Task<Result<Message>> Handle(ProcessPickupStatusRequestRequest request, CancellationToken cancellationToken)
    {
        var body = request.UnpackedMessage.Body;
        var hasRecipientDid = body.TryGetValue("recipient_did", out var recipientDidBody);
        string? recipientDid = null;
        if (hasRecipientDid)
        {
            var recipientDidJsonElement = (JsonElement)recipientDidBody;
            if (recipientDidJsonElement.ValueKind is JsonValueKind.String)
            {
                //TODO check for valid did
                recipientDid = recipientDidJsonElement.GetString();
            }
            else
            {
                return Result.Fail("Invalid body format: recipient_did");
            }
        }

        var getStatusResult = await _mediator.Send(new GetMessagesStatusRequest(request.SenderDid, request.MediatorDid, recipientDid), cancellationToken);
        if (getStatusResult.IsFailed)
        {
            return getStatusResult.ToResult();
        }

        var returnBody = new Dictionary<string, object>();
        returnBody.Add("message_count", getStatusResult.Value.MessageCount);
        if (getStatusResult.Value.RecipientDid is not null)
        {
            returnBody.Add("recipient_did", getStatusResult.Value.RecipientDid!);
        }

        if (getStatusResult.Value.LongestWaitedSeconds is not null)
        {
            returnBody.Add("longest_waited_seconds", getStatusResult.Value.LongestWaitedSeconds!);
        }

        if (getStatusResult.Value.NewestMessageTime is not null)
        {
            returnBody.Add("NewestMessageTime", getStatusResult.Value.NewestMessageTime!);
        }

        if (getStatusResult.Value.OldestMessageTime is not null)
        {
            returnBody.Add("oldest_received_time", getStatusResult.Value.OldestMessageTime!);
        }

        if (getStatusResult.Value.TotalByteSize is not null)
        {
            returnBody.Add("total_bytes", getStatusResult.Value.TotalByteSize!);
        }

        //TODO
        returnBody.Add("live_delivery", false);

        var statusMessage = new MessageBuilder(
                id: Guid.NewGuid().ToString(),
                type: ProtocolConstants.MessagePickup3StatusResponse,
                body: returnBody 
            )
            .fromPrior(request.FromPrior)
            .build();
        return Result.Ok(statusMessage);
    }
}