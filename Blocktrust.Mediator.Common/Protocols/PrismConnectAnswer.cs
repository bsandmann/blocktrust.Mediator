namespace Blocktrust.Mediator.Common.Protocols;

using DIDComm.Message.Messages;

public static class PrismConnectAnswer
{
    public static Message Create()
    {
        var body = new Dictionary<string, object>();
        // TODO are they needed?
        // body.Add("goal_code", GoalCodes.PrismConnect);
        // body.Add("goal", "Connect");
        // body.Add("accept", "didcomm/v2");
        var basicMessage = new MessageBuilder(
                id: Guid.NewGuid().ToString(),
                type: ProtocolConstants.PrismConnectResponse,
                body: body
            )
            .createdTime(new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds())
            .build();
        return basicMessage;
    } 
}