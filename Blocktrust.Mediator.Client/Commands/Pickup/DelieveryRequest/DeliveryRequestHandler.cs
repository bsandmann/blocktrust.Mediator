namespace Blocktrust.Mediator.Client.Commands.Pickup.StatusRequest;

using System.Net;
using System.Text;
using System.Text.Json;
using Blocktrust.Common.Resolver;
using Common.Models.Pickup;
using Common.Protocols;
using DelieveryRequest;
using DIDComm;
using DIDComm.Common.Types;
using DIDComm.Message.Attachments;
using DIDComm.Message.Messages;
using DIDComm.Model.PackEncryptedParamsModels;
using DIDComm.Model.UnpackParamsModels;
using DIDComm.Utils;
using FluentResults;
using ForwardMessage;
using MediatR;

public class DeliveryRequestHandler : IRequestHandler<DeliveryRequestRequest, Result<DeliveryRequestResponse>>
{
    private readonly IMediator _mediator;
    private readonly HttpClient _httpClient;
    private readonly IDidDocResolver _didDocResolver;
    private readonly ISecretResolver _secretResolver;

    public DeliveryRequestHandler(IMediator mediator, HttpClient httpClient, IDidDocResolver didDocResolver, ISecretResolver secretResolver)
    {
        _mediator = mediator;
        _httpClient = httpClient;
        _didDocResolver = didDocResolver;
        _secretResolver = secretResolver;
    }

    public async Task<Result<DeliveryRequestResponse>> Handle(DeliveryRequestRequest request, CancellationToken cancellationToken)
    {
        var body = new Dictionary<string, object>();
        if (!string.IsNullOrEmpty(request.RecipientDid))
        {
            body.Add("recipient_did", request.RecipientDid);
        }

        body.Add("limit", request.Limit);

        var statusRequestMessage = new MessageBuilder(
                id: Guid.NewGuid().ToString(),
                type: ProtocolConstants.MessagePickup3DeliveryRequest,
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

        if (unpackResult.Value.Message.Type != ProtocolConstants.MessagePickup3DeliveryResponse)
        {
            return Result.Fail($"Unexpected header-type: {unpackResult.Value.Message.Type}");
        }


        var attachments = unpackResult.Value.Message.Attachments;
        if (!attachments.Any())
        {
            //if we don't have any attachments, we should have a status message in the body
            var bodyContent = unpackResult.Value.Message.Body;
            //TOOD parse that
            var statusMessage = new StatusRequestResponse();
            return Result.Ok(new DeliveryRequestResponse(statusMessage));
        }


        var messages = new List<DeliveryResponseModel>();
        foreach (var attachment in attachments)
        {
            var data = attachment.Data;
            var id = attachment.Id;
            var innerMessage = string.Empty;
            if (data is Json)
            {
                Json? jsonAttachmentData = (Json)data;
                var innerJson = jsonAttachmentData?.JsonString;
                var msg = innerJson?.GetTyped<Dictionary<string, object>>("json");
                innerMessage = JsonSerializer.Serialize(msg);
            }
            else
            {
                throw new NotImplementedException("Not implemented yet");
            }

            var unpackInnerResult = didComm.Unpack(
                new UnpackParamsBuilder(innerMessage)
                    .SecretResolver(_secretResolver)
                    .BuildUnpackParams());

            if (unpackResult.IsFailed)
            {
                messages.Add(new DeliveryResponseModel(unpackResult.Errors.FirstOrDefault()?.Message));
            }
            else
            {
                messages.Add(new DeliveryResponseModel(id, unpackInnerResult.Value.Message));
            }
        }

        return Result.Ok(new DeliveryRequestResponse(messages));
    }
}