namespace Blocktrust.Mediator.Server.Commands.Pickup.ProcessStatusRequest;

using System.Text.Json;
using Blocktrust.DIDComm.Message.Messages;
using Blocktrust.Mediator.Common.Protocols;
using Blocktrust.Mediator.Server.Commands.DatabaseCommands.GetMessagesStatus;
using FluentResults;
using MediatR;

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

    /// <inheritdoc />
    public async Task<Result<Message>> Handle(ProcessPickupStatusRequestRequest request, CancellationToken cancellationToken)
    {
        var body = request.UnpackedMessage.Body;
        var hasRecipientDid = body.TryGetValue("recipient_did", out var recipientDidBody);
        string? recipientDid = null;
        if (hasRecipientDid)
        {
            var recipientDidJsonElement = (JsonElement)recipientDidBody!;
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

        var statusMessage = new MessageBuilder(
                id: Guid.NewGuid().ToString(),
                type: ProtocolConstants.MessagePickup3StatusResponse,
                body: getStatusResult.Value.GetMessagePickup3StatusResponseBody()
            )
            .fromPrior(request.FromPrior)
            .build();
        return Result.Ok(statusMessage);
    }
}