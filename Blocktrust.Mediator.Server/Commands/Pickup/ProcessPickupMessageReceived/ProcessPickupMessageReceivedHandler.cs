namespace Blocktrust.Mediator.Server.Commands.Pickup.ProcessPickupDeliveryRequest;

using System.Text.Json;
using Blocktrust.DIDComm.Message.Messages;
using Blocktrust.Mediator.Common.Protocols;
using Blocktrust.Mediator.Server.Commands.DatabaseCommands.GetMessagesStatus;
using DatabaseCommands.DeleteMessages;
using FluentResults;
using MediatR;
using ProcessPickupMessageReceived;

public class ProcessPickupMessageReceivedHandler : IRequestHandler<ProcessPickupMessageReceivedRequest, Result<Message>>
{
    private readonly IMediator _mediator;

    /// <summary>
    /// Constructor
    /// </summary>
    public ProcessPickupMessageReceivedHandler(IMediator mediator)
    {
        this._mediator = mediator;
    }

    /// <inheritdoc />
    public async Task<Result<Message>> Handle(ProcessPickupMessageReceivedRequest request, CancellationToken cancellationToken)
    {
        var body = request.UnpackedMessage.Body;
        var hasMessageIdList = body.TryGetValue("message_id_list", out var messageIdListJson);
        if (!hasMessageIdList)
        {
            return Result.Fail("Invalid body format: missing 'message_id_list'");
        }

        var messageIdListJsonElement = (JsonElement)messageIdListJson!;
        var messageIdList = new List<string>();
        if (messageIdListJsonElement.ValueKind is JsonValueKind.Array)
        {
            foreach (var idJsonElement in messageIdListJsonElement.EnumerateArray())
            {
                if (idJsonElement.ValueKind is not JsonValueKind.String)
                {
                    return Result.Fail("Invalid body format: incorrect entries in 'message_id_list'");
                }
                messageIdList.Add(idJsonElement!.GetString());
            }
        }
        else
        {
            return Result.Fail("Invalid body format: message_id_list");
        }

        var deleteMessagesResult = await _mediator.Send(new DeleteMessagesRequest(request.SenderDid, request.MediatorDid, messageIdList), cancellationToken);
        if (deleteMessagesResult.IsFailed)
        {
            return deleteMessagesResult;
        }
        
        var getStatusResult = await _mediator.Send(new GetMessagesStatusRequest(request.SenderDid, request.MediatorDid, null), cancellationToken);
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