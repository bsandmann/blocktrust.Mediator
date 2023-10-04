namespace Blocktrust.Mediator.Client.Commands.Pickup.DeliveryRequest;

using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Blocktrust.Common.Converter;
using Blocktrust.Common.Resolver;
using Blocktrust.DIDComm;
using Blocktrust.DIDComm.Common.Types;
using Blocktrust.DIDComm.Message.Attachments;
using Blocktrust.DIDComm.Message.Messages;
using Blocktrust.DIDComm.Model.PackEncryptedParamsModels;
using Blocktrust.DIDComm.Model.UnpackParamsModels;
using Blocktrust.DIDComm.Utils;
using Blocktrust.Mediator.Common.Models.Pickup;
using Blocktrust.Mediator.Common.Protocols;
using Common.Models.ProblemReport;
using FluentResults;
using MediatR;

public class DeliveryRequestHandler : IRequestHandler<DeliveryRequestRequest, Result<DeliveryRequestResponse>>
{
    private readonly HttpClient _httpClient;
    private readonly IDidDocResolver _didDocResolver;
    private readonly ISecretResolver _secretResolver;

    public DeliveryRequestHandler(HttpClient httpClient, IDidDocResolver didDocResolver, ISecretResolver secretResolver)
    {
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
            .returnRoute("all")
            .to(new List<string>() { request.MediatorDid })
            .from(request.LocalDid)
            .build();

        var didComm = new DidComm(_didDocResolver, _secretResolver);

        // We pack the message and encrypt it for the mediator
        var packResult = await didComm.PackEncrypted(
            new PackEncryptedParamsBuilder(statusRequestMessage, to: request.MediatorDid)
                .From(request.LocalDid)
                .ProtectSenderId(false)
                .BuildPackEncryptedParams()
        );

        if (packResult.IsFailed)
        {
            return packResult.ToResult();
        }

        // We send the message to the mediator
        HttpResponseMessage response;
        try
        {
            response = await _httpClient.PostAsync(request.MediatorEndpoint, new StringContent(packResult.Value.PackedMessage, new MediaTypeHeaderValue(MessageTyp.Encrypted)), cancellationToken);
        }
        catch (HttpRequestException ex)
        {
            return Result.Fail($"Connection could not be established: {ex.Message}");
        }

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return Result.Fail("Connection could not be established");
        }
        else if (!response.IsSuccessStatusCode)
        {
            return Result.Fail("Unable to initiate connection: " + response.StatusCode);
        }

        var content = await response.Content.ReadAsStringAsync(cancellationToken);

        var unpackResult = await didComm.Unpack(
            new UnpackParamsBuilder(content)
                .SecretResolver(_secretResolver)
                .BuildUnpackParams());

        if (unpackResult.IsFailed)
        {
            return unpackResult.ToResult();
        }

        if (unpackResult.Value.Message.Type == ProtocolConstants.ProblemReport)
        {
            if (unpackResult.Value.Message.Pthid != null)
            {
                var problemReport = ProblemReport.Parse(unpackResult.Value.Message.Body, unpackResult.Value.Message.Pthid);
                if (problemReport.IsFailed)
                {
                    return Result.Fail("Error parsing the problem report of the mediator");
                }

                return Result.Ok(new DeliveryRequestResponse(problemReport.Value));
            }

            return Result.Fail("Error parsing the problem report of the mediator. Missing parent-thread-id");
        }

        if (unpackResult.Value.Message.Type == ProtocolConstants.MessagePickup3StatusResponse)
        {
            var bodyContent = unpackResult.Value.Message.Body;
            var statusRequestResponseResult = StatusRequestResponse.Parse(bodyContent);
            if (statusRequestResponseResult.IsFailed)
            {
                return Result.Fail("Error parsing the status response of the mediator");
            }

            return Result.Ok(new DeliveryRequestResponse(statusRequestResponseResult.Value));
        }


        if (unpackResult.Value.Message.Type != ProtocolConstants.MessagePickup3DeliveryResponse)
        {
            return Result.Fail($"Unexpected header-type: {unpackResult.Value.Message.Type}");
        }

        var attachments = unpackResult.Value.Message.Attachments;
        if (attachments is null || !attachments.Any())
        {
            // if we don't have any attachments, we should have a status message in the body
            // so this case should ever happen, since the MessageType would then be StatusResponse

            // commented out for now, since this is currently causing compatibility issues with the roots mediator
            return Result.Fail("Invalid response from mediator. No attachments found.");
        }


        var messages = new List<DeliveryResponseModel>();
        foreach (var attachment in attachments)
        {
            var data = attachment.Data;
            var id = attachment.Id;
            string innerMessage;
            if (data is Json)
            {
                Json? jsonAttachmentData = (Json)data;
                var innerJson = jsonAttachmentData?.JsonString;
                innerMessage = JsonSerializer.Serialize(innerJson, SerializationOptions.UnsafeRelaxedEscaping);
            }
            else if (data is Base64)
            {
                Base64? base64AttachmentData = (Base64)data;
                var inner = Base64Url.Decode(base64AttachmentData.Base64String);
                innerMessage = Encoding.UTF8.GetString(inner);
            }
            else
            {
                throw new NotImplementedException("Not implemented yet");
            }

            var unpackInnerResult = await didComm.Unpack(
                new UnpackParamsBuilder(innerMessage)
                    .SecretResolver(_secretResolver)
                    .BuildUnpackParams());

            if (unpackInnerResult.IsFailed)
            {
                messages.Add(new DeliveryResponseModel(unpackResult.Errors!.FirstOrDefault()?.Message));
            }
            else
            {
                messages.Add(new DeliveryResponseModel(id, unpackInnerResult.Value.Message, unpackInnerResult.Value.Metadata));
            }
        }

        return Result.Ok(new DeliveryRequestResponse(messages));
    }
}