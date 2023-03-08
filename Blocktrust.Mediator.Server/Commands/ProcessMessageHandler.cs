namespace Blocktrust.Mediator.Server.Commands;

using Common.Commands.CreatePeerDid;
using Common.Models.ProblemReport;
using Common.Protocols;
using DatabaseCommands.CreateConnection;
using DatabaseCommands.GetConnection;
using DIDComm.Message.FromPriors;
using DIDComm.Message.Messages;
using FluentResults;
using ForwardMessage;
using MediatorCoordinator.ProcessMediationRequest;
using MediatorCoordinator.ProcessQueryMediatorKeys;
using MediatorCoordinator.ProcessUpdateMediatorKeys;
using MediatR;
using Pickup.ProcessPickupDeliveryRequest;
using Pickup.ProcessPickupLiveDeliveryChange;
using Pickup.ProcessPickupMessageReceived;
using Pickup.ProcessStatusRequest;
using ProcessMessage;

public class ProcessMessageHandler : IRequestHandler<ProcessMessageRequest, ProcessMessageResponse>
{
    private readonly IMediator _mediator;

    /// <summary>
    /// Constructor
    /// </summary>
    public ProcessMessageHandler(IMediator mediator)
    {
        this._mediator = mediator;
    }

    /// <inheritdoc />
    public async Task<ProcessMessageResponse> Handle(ProcessMessageRequest request, CancellationToken cancellationToken)
    {
        FromPrior? fromPrior = null;
        string mediatorDid;
        var fallBackMediatorDidForErrorMessages = request.UnpackResult.Metadata.EncryptedTo.FirstOrDefault();
        if (fallBackMediatorDidForErrorMessages is null)
        {
            //TODO no clue
        }

        var existingConnection = await _mediator.Send(new GetConnectionRequest(request.SenderOldDid, null));
        if (existingConnection.IsFailed)
        {
            return new ProcessMessageResponse(
                ProblemReportMessage.BuildDefaultInternalError(
                    errorMessage: "Unable to connect to database",
                    threadIdWhichChausedTheProblem: request.UnpackResult.Message.Thid ?? request.UnpackResult.Message.Id,
                    fromPrior: fromPrior), fallBackMediatorDidForErrorMessages);
        }

        if (existingConnection.Value is null)
        {
            // Create new connection
            var mediatorDidResult = await _mediator.Send(new CreatePeerDidRequest(serviceEndpoint: new Uri(request.HostUrl)));
            if (mediatorDidResult.IsFailed)
            {
                return new ProcessMessageResponse(
                    ProblemReportMessage.BuildDefaultInternalError(
                        errorMessage: "Unable to create PeerDID",
                        threadIdWhichChausedTheProblem: request.UnpackResult.Message.Thid ?? request.UnpackResult.Message.Id,
                        fromPrior: fromPrior), fallBackMediatorDidForErrorMessages);
            }

            var iss = request.UnpackResult.Metadata.EncryptedTo.First().Split('#')[0]; // The current Did of the mediator the msg was send to
            var sub = mediatorDidResult.Value.PeerDid.Value; // The new Did of the mediator that will be used for future communication
            fromPrior = FromPrior.Builder(iss, sub).Build();

            var createConnectionResult = await _mediator.Send(new CreateConnectionRequest(mediatorDidResult.Value.PeerDid.Value, request.SenderDid));
            if (createConnectionResult.IsFailed)
            {
                return new ProcessMessageResponse(
                    ProblemReportMessage.BuildDefaultInternalError(
                        errorMessage: "Unable grant mediation",
                        threadIdWhichChausedTheProblem: request.UnpackResult.Message.Thid ?? request.UnpackResult.Message.Id,
                        fromPrior: fromPrior), fallBackMediatorDidForErrorMessages);
            }

            mediatorDid = mediatorDidResult.Value.PeerDid.Value;
        }
        else
        {
            mediatorDid = existingConnection.Value.MediatorDid;
        }

        Result<Message> result = Result.Fail(string.Empty);
        switch (request.UnpackResult.Message.Type)
        {
            case ProtocolConstants.CoordinateMediation2Request:
                result = await _mediator.Send(new ProcessMediationRequestRequest(request.UnpackResult.Message, request.SenderDid, mediatorDid, request.HostUrl, fromPrior));
                break;
            case ProtocolConstants.CoordinateMediation2KeylistUpdate:
                result = await _mediator.Send(new ProcessUpdateMediatorKeysRequest(request.UnpackResult.Message, request.SenderDid, mediatorDid, request.HostUrl, fromPrior));
                break;
            case ProtocolConstants.CoordinateMediation2KeylistQuery:
                result = await _mediator.Send(new ProcessQueryMediatorKeysRequest(request.UnpackResult.Message, request.SenderDid, mediatorDid, request.HostUrl, fromPrior));
                break;
            case ProtocolConstants.MessagePickup3StatusRequest:
                result = await _mediator.Send(new ProcessPickupStatusRequestRequest(request.UnpackResult.Message, request.SenderDid, mediatorDid, request.HostUrl, fromPrior));
                break;
            case ProtocolConstants.MessagePickup3DeliveryRequest:
                result = await _mediator.Send(new ProcessPickupDeliveryRequestRequest(request.UnpackResult.Message, request.SenderDid, mediatorDid, request.HostUrl, fromPrior));
                break;
            case ProtocolConstants.MessagePickup3MessagesReceived:
                result = await _mediator.Send(new ProcessPickupMessageReceivedRequest(request.UnpackResult.Message, request.SenderDid, mediatorDid, request.HostUrl, fromPrior));
                break;
            case ProtocolConstants.MessagePickup3LiveDeliveryChange:
                result = await _mediator.Send(new ProcessPickupDeliveryChangeRequest(request.UnpackResult.Message, request.SenderDid, mediatorDid, request.HostUrl, fromPrior));
                break;
            case ProtocolConstants.ForwardMessage:
            {
                result = await _mediator.Send(new ProcessForwardMessageRequest(request.UnpackResult.Message, request.SenderDid, mediatorDid, request.HostUrl, fromPrior));
                if (result.IsFailed)
                {
                    return new ProcessMessageResponse(
                        ProblemReportMessage.BuildDefaultInternalError(
                            errorMessage: result.Errors.First().Message,
                            threadIdWhichChausedTheProblem: request.UnpackResult.Message.Thid ?? request.UnpackResult.Message.Id,
                            fromPrior: fromPrior), mediatorDid);
                }

                return new ProcessMessageResponse(); // 202 Accecpted
            }
            default:
                return new ProcessMessageResponse(ProblemReportMessage.Build(
                    fromPrior: fromPrior,
                    threadIdWhichCausedTheProblem: request.UnpackResult.Message.Thid ?? request.UnpackResult.Message.Id,
                    problemCode: new ProblemCode(
                        sorter: EnumProblemCodeSorter.Error,
                        scope: EnumProblemCodeScope.Message,
                        stateNameForScope: null,
                        descriptor: EnumProblemCodeDescriptor.Message,
                        otherDescriptor: null
                    ),
                    comment: $"Not supported message type: '{request.UnpackResult.Message.Type}'",
                    commentArguments: null,
                    escalateTo: new Uri("mailto:info@blocktrust.dev")), mediatorDid);
        }

        return new ProcessMessageResponse(result.Value, mediatorDid);
    }
}