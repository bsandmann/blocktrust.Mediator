namespace Blocktrust.Mediator.Server.Commands.DiscoverFeatures;

using System.Text.Json;
using Blocktrust.DIDComm.Message.Messages;
using Blocktrust.Mediator.Common.Protocols;
using MediatR;
using TrustPing;

public class ProcessTrustPingHandler : IRequestHandler<ProcessTrustPingRequest, Message?>
{
    /// <inheritdoc />
    public async Task<Message?> Handle(ProcessTrustPingRequest request, CancellationToken cancellationToken)
    {
        var body = request.UnpackedMessage.Body;
        var hasResponseRequestedFlag = body.TryGetValue("response_requested", out var repsonseRequestedFlagJson);
        if (!hasResponseRequestedFlag)
        {
            return null;
        }

        var responseRequestedFlagJsonElement = (JsonElement)repsonseRequestedFlagJson;
        bool responseRequestedFlag = false;
        if (responseRequestedFlagJsonElement.ValueKind == JsonValueKind.True || responseRequestedFlagJsonElement.ValueKind == JsonValueKind.False)
        {
            responseRequestedFlag = responseRequestedFlagJsonElement.GetBoolean();
        }

        if (!responseRequestedFlag)
        {
            return null;
        }

        var disclosureMessage = new MessageBuilder(
                id: Guid.NewGuid().ToString(),
                type: ProtocolConstants.TrustPingResponse,
                body: new Dictionary<string, object>()
            )
            .thid(request.UnpackedMessage.Thid ?? request.UnpackedMessage.Id)
            .fromPrior(request.FromPrior)
            .build();

        return disclosureMessage;
    }
}