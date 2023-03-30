namespace Blocktrust.Mediator.Client.Commands.PrismConnect.AnwserConnectRequest;

using Blocktrust.PeerDID.Types;
using FluentResults;
using MediatR;

/// <summary>
/// The request handles the scenario where the local agent offered a OOB invitation to a PRISM agent.
/// The local-agent is now waiting for the PRISM agent to answer the invitation and send a connect-request to our mediator.
/// This code waits until the mediator reveives the connect-request and then answers the connect-request.
/// </summary>
public class AnswerPrismConnectRequest : IRequest<Result<AnswerPrismConnectResponse>>
{
    /// <summary>
    /// The PeerDid which was used inside the OOB invitation, send to the PRISM agent
    /// </summary>
    public PeerDid LocalPeerDidUsedInOobInvitation { get; }

    /// <summary>
    /// The endpoint of the mediator used by the local PeerDid
    /// </summary>
    public Uri MediatorEndpoint { get; }

    /// <summary>
    /// The Mediator DID used by the local PeerDid
    /// </summary>
    public string MediatorDid { get; }

    /// <summary>
    /// The local DID used to communicate with the mediator
    /// </summary>
    public string LocalDidToUseWithMediator { get; }

    /// <summary>
    /// The maximal time to wait for the connect-request to arrive at the mediator
    /// </summary>
    public TimeSpan MaxTimeToWait { get; }

    /// <summary>
    /// The messageId which was used inside the OOB-invitation
    /// </summary>
    public string MessageId { get; }

    public AnswerPrismConnectRequest(PeerDid localPeerDidUsedInOobInvitation, string localDidToUseWithMediator, Uri mediatorEndpoint, string mediatorDid, TimeSpan maxTimeToWait, string messageId)
    {
        LocalPeerDidUsedInOobInvitation = localPeerDidUsedInOobInvitation;
        MediatorEndpoint = mediatorEndpoint;
        LocalDidToUseWithMediator = localDidToUseWithMediator;
        MediatorDid = mediatorDid;
        MaxTimeToWait = maxTimeToWait;
        MessageId = messageId;
    }
}