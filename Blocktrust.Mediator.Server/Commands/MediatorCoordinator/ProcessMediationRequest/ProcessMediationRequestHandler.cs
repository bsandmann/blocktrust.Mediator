namespace Blocktrust.Mediator.Server.Commands.MediatorCoordinator.ProcessMediationRequest;

using Blocktrust.DIDComm.Message.Messages;
using Blocktrust.Mediator.Common.Commands.CreatePeerDid;
using Blocktrust.Mediator.Common.Protocols;
using Common.Models.ProblemReport;
using DatabaseCommands.GetConnection;
using DatabaseCommands.UpdateConnection;
using MediatR;

public class ProcessMediationRequestHandler : IRequestHandler<ProcessMediationRequestRequest, Message>
{
    private readonly IMediator _mediator;

    /// <summary>
    /// Constructor
    /// </summary>
    public ProcessMediationRequestHandler(IMediator mediator)
    {
        this._mediator = mediator;
    }

    /// <inheritdoc />
    public async Task<Message> Handle(ProcessMediationRequestRequest request, CancellationToken cancellationToken)
    {
        // If we have a problem establishing a connection to the database, we return a problem report message 
        var existingConnection = await _mediator.Send(new GetConnectionRequest(request.SenderDid, request.MediatorDid), cancellationToken);
        if (existingConnection.IsFailed)
        {
            return ProblemReportMessage.BuildDefaultInternalError(
                errorMessage: "Unknown database error",
                threadIdWhichCausedTheProblem: request.UnpackedMessage.Thid ?? request.UnpackedMessage.Id,
                fromPrior: request.FromPrior);
        }

        if (existingConnection.Value is not null && existingConnection.Value.MediationGranted)
        {
            // we already have a mediation. deny the request
            var mediateDenyMessage = new MessageBuilder(
                    id: Guid.NewGuid().ToString(),
                    type: ProtocolConstants.CoordinateMediation2Deny,
                    body: new Dictionary<string, object>()
                )
                .thid(request.UnpackedMessage.Thid ?? request.UnpackedMessage.Id)
                .fromPrior(request.FromPrior)
                .build();
            return mediateDenyMessage;
        }
        else
        {
            var routingDidResult = await _mediator.Send(new CreatePeerDidRequest(serviceEndpoint: new Uri(request.HostUrl)), cancellationToken);
            if (routingDidResult.IsFailed)
            {
                return ProblemReportMessage.BuildDefaultInternalError(
                    errorMessage: "Unable to connect to database",
                    threadIdWhichCausedTheProblem: request.UnpackedMessage.Thid ?? request.UnpackedMessage.Id,
                    fromPrior: request.FromPrior);
            }
            
            if (request.SenderDid is null || request.MediatorDid is null)
            {
                return ProblemReportMessage.BuildDefaultMessageMissingArguments(
                    errorMessage: "Invalid body format: missing sender_did or mediator_did",
                    threadIdWhichCausedTheProblem: request.UnpackedMessage.Thid ?? request.UnpackedMessage.Id,
                    fromPrior: request.FromPrior);
            }

            var connectionResult = await _mediator.Send(new UpdateConnectionMediationRequest(mediatorDid: request.MediatorDid,
                remoteDid: request.SenderDid,
                routingDid: routingDidResult.Value.PeerDid.Value,
                mediatorEndpoint: request.HostUrl,
                mediationGranted: true), cancellationToken);

            if (connectionResult.IsFailed)
            {
                return ProblemReportMessage.BuildDefaultInternalError(
                    errorMessage: "Database update failed",
                    threadIdWhichCausedTheProblem: request.UnpackedMessage.Thid ?? request.UnpackedMessage.Id,
                    fromPrior: request.FromPrior);
            }

            // Create the grant mediation message
            var mediateGrantMessage = new MessageBuilder(
                    id: Guid.NewGuid().ToString(),
                    type: ProtocolConstants.CoordinateMediation2Grant,
                    body: new Dictionary<string, object>()
                    {
                        { "routing_did", routingDidResult.Value.PeerDid.Value }
                    }
                )
                .thid(request.UnpackedMessage.Thid ?? request.UnpackedMessage.Id)
                .fromPrior(request.FromPrior)
                .build();
            return mediateGrantMessage;
        }
    }
}