namespace Blocktrust.Mediator.Client.Commands.PrismConnect.CreateOobInvitation;

using Common;
using Common.Models.OutOfBand;
using PeerDID.Types;

public static class PrismConnectOobInvitation
{
    public static (string invitation, string messageId) Create(PeerDid localPeerDid)
    {
        var goal = "Establish a trust connection between two peers using the protocol 'https://atalaprism.io/mercury/connections/1.0/request'";
        return OobModel.BuildGenericOobMessage(localPeerDid, goalCode: GoalCodes.PrismConnectOob, goal: goal);
    }
}