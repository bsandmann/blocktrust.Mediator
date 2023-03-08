namespace Blocktrust.Mediator.Client.Commands.Pickup.MessageReceived;

using System.Net;
using System.Text;
using Blocktrust.Common.Resolver;
using Common.Models.Pickup;
using Common.Models.ProblemReport;
using Common.Protocols;
using DIDComm;
using DIDComm.Common.Types;
using DIDComm.Message.Messages;
using DIDComm.Model.PackEncryptedParamsModels;
using DIDComm.Model.UnpackParamsModels;
using FluentResults;
using MediatR;
using StatusRequest;

public class LiveDeliveryChangeHandler : IRequestHandler<LiveDeliveryChangeRequest, Result<ProblemReport>>
{
    private readonly HttpClient _httpClient;
    private readonly IDidDocResolver _didDocResolver;
    private readonly ISecretResolver _secretResolver;

    public LiveDeliveryChangeHandler(HttpClient httpClient, IDidDocResolver didDocResolver, ISecretResolver secretResolver)
    {
        _httpClient = httpClient;
        _didDocResolver = didDocResolver;
        _secretResolver = secretResolver;
    }

    public async Task<Result<ProblemReport>> Handle(LiveDeliveryChangeRequest request, CancellationToken cancellationToken)
    {
        var body = new Dictionary<string, object>();
        body.Add("live_delivery", request.LiveDelivery);

        var statusRequestMessage = new MessageBuilder(
                id: Guid.NewGuid().ToString(),
                type: ProtocolConstants.MessagePickup3LiveDeliveryChange,
                body: body
            )
            .to(new List<string>() { request.MediatorDid })
            .build();

        var didComm = new DidComm(_didDocResolver, _secretResolver);

        // We pack the message and encrypt it for the mediator
        var packResult = didComm.PackEncrypted(
            new PackEncryptedParamsBuilder(statusRequestMessage, to: request.MediatorDid)
                .From(request.LocalDid)
                .ProtectSenderId(false)
                .BuildPackEncryptedParams()
        );

        // We send the message to the mediator endpoint
        var response = await _httpClient.PostAsync(request.MediatorEndpoint, new StringContent(packResult.PackedMessage, Encoding.UTF8, MessageTyp.Encrypted), cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return Result.Fail("Connection could not be established");
        }
        else if (!response.IsSuccessStatusCode)
        {
            return Result.Fail("Unable to initiate connection: " + response.StatusCode);
        }

        var content = await response.Content.ReadAsStringAsync(cancellationToken);

        var unpackResult = didComm.Unpack(
            new UnpackParamsBuilder(content)
                .SecretResolver(_secretResolver)
                .BuildUnpackParams());

        if (unpackResult.IsFailed)
        {
            return unpackResult.ToResult();
        }

        //TODO this is very much just a test to test the compatiblity - not of the expected behaviour for a production system!

        if (unpackResult.Value.Message.Type == ProtocolConstants.ProblemReport)
        {
            if (unpackResult.Value.Message.Pthid != null)
            {
                var problemReport = ProblemReport.Parse(unpackResult.Value.Message.Body, unpackResult.Value.Message.Pthid);
                if (problemReport.IsFailed)
                {
                    return Result.Fail("Error parsing the problem report of the mediator");
                }
                
                return Result.Ok(problemReport.Value);
            }
        }

        return Result.Ok();
    }
}