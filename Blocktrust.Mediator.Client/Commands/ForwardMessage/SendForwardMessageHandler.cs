namespace Blocktrust.Mediator.Client.Commands.ForwardMessage;

using System.Net;
using System.Text;
using Blocktrust.Common.Resolver;
using Common.Models.ProblemReport;
using Common.Protocols;
using DIDComm;
using DIDComm.Common.Types;
using DIDComm.Message.Attachments;
using DIDComm.Message.Messages;
using DIDComm.Model.PackEncryptedParamsModels;
using DIDComm.Model.UnpackParamsModels;
using FluentResults;
using MediatR;
using Json = DIDComm.Message.Attachments.Json;

public class SendForwardMessageHandler : IRequestHandler<SendForwardMessageRequest, Result<ProblemReport>>
{
    private readonly HttpClient _httpClient;
    private readonly IDidDocResolver _didDocResolver;
    private readonly ISecretResolver _secretResolver;

    public SendForwardMessageHandler(HttpClient httpClient, IDidDocResolver didDocResolver, ISecretResolver secretResolver)
    {
        _httpClient = httpClient;
        _didDocResolver = didDocResolver;
        _secretResolver = secretResolver;
    }

    public async Task<Result<ProblemReport>> Handle(SendForwardMessageRequest request, CancellationToken cancellationToken)
    {
        // https://identity.foundation/didcomm-messaging/spec/#using-a-did-as-an-endpoint
        // This implementation doesn't support "Exmaple 2". That is double-wrapping the message

        // We create the wrapping message, with has the inner message in the attachments
        Dictionary<string, object> packedMessage;
        try
        {
            packedMessage = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(request.Message);
        }
        catch (Exception e)
        {
            return Result.Fail($"The message cannot be deserialized: {e}");
        }

        if (packedMessage is null)
        {
            return Result.Fail("The message cannot be deserialized");
        }

        var attachments = new List<Attachment>
        {
            new AttachmentBuilder(
                id: Guid.NewGuid().ToString(),
                data: new Json(json: packedMessage)
            ).Build()
        };
        var wrappedMessage = new MessageBuilder(
                id: Guid.NewGuid().ToString(),
                type: ProtocolConstants.ForwardMessage,
                body: new Dictionary<string, object>()
                {
                    { "next", request.RecipientDid }
                }
            )
            .attachments(attachments)
            .to(new List<string>() { request.MediatorDid })
            .build();


        var didComm = new DidComm(_didDocResolver, _secretResolver);

        // We pack the message and encrypt it for the mediator
        var packResult = await didComm.PackEncrypted(
            new PackEncryptedParamsBuilder(wrappedMessage, to: request.MediatorDid)
                .From(request.LocalDid)
                .ProtectSenderId(false)
                .BuildPackEncryptedParams()
        );

        // We send the message to the mediator
        HttpResponseMessage response;
        try
        {
            response = await _httpClient.PostAsync(request.MediatorEndpoint, new StringContent(packResult.PackedMessage, Encoding.UTF8, MessageTyp.Encrypted), cancellationToken);
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
        else if (response.StatusCode == HttpStatusCode.Accepted)
        {
            return Result.Ok();
        }
        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        
        if (!string.IsNullOrEmpty(content))
        {
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

                    return Result.Ok(problemReport.Value);
                }

                return Result.Fail("Error parsing the problem report of the mediator. Missing parent-thread-id");
            }
        } 
        
        
        return Result.Fail("The result code should be 202! This is not really a fail here, but anyway....");
    }
}