namespace Blocktrust.Mediator.Server.Commands.Pickup.ProcessPickupLiveDeliveryChange;

using Blocktrust.DIDComm.Message.Messages;
using Blocktrust.Mediator.Common.Models.ProblemReport;
using MediatR;

public class ProcessPickupDeliveryChangeHandler : IRequestHandler<ProcessPickupDeliveryChangeRequest, Message>
{
    private readonly IMediator _mediator;

    /// <summary>
    /// Constructor
    /// </summary>
    public ProcessPickupDeliveryChangeHandler(IMediator mediator)
    {
        this._mediator = mediator;
    }

    /// <inheritdoc />
    public async Task<Message> Handle(ProcessPickupDeliveryChangeRequest request, CancellationToken cancellationToken)
    {
        var threadId = request.UnpackedMessage.Thid ?? request.UnpackedMessage.Id;
        var problemReport = ProblemReportMessage.Build(
            fromPrior: request.FromPrior,
            threadIdWhichCausedTheProblem: threadId,
            problemCode: new ProblemCode(
                sorter: EnumProblemCodeSorter.Error,
                scope: EnumProblemCodeScope.Message,
                stateNameForScope: null,
                descriptor: EnumProblemCodeDescriptor.Other,
                otherDescriptor: "live-delivery-not-supported"
            ),
            comment: "Connection does not support Live Delivery",
            commentArguments: null,
            escalateTo: new Uri("mailto:info@blocktrust.dev"));

        return problemReport;
    }
}