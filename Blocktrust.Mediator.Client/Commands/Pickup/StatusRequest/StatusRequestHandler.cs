namespace Blocktrust.Mediator.Client.Commands.Pickup.StatusRequest;

using System.Net;
using System.Net.Http.Headers;
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

public class StatusRequestHandler : IRequestHandler<StatusRequestRequest, Result<StatusRequestResponse>>
{
    private readonly HttpClient _httpClient;
    private readonly IDidDocResolver _didDocResolver;
    private readonly ISecretResolver _secretResolver;

    public StatusRequestHandler(HttpClient httpClient, IDidDocResolver didDocResolver, ISecretResolver secretResolver)
    {
        _httpClient = httpClient;
        _didDocResolver = didDocResolver;
        _secretResolver = secretResolver;
    }

    public async Task<Result<StatusRequestResponse>> Handle(StatusRequestRequest request, CancellationToken cancellationToken)
    {
        var body = new Dictionary<string, object>();
        if (!string.IsNullOrEmpty(request.RecipientDid))
        {
            body.Add("recipient_did", request.RecipientDid);
        }

        var statusRequestMessage = new MessageBuilder(
                id: Guid.NewGuid().ToString(),
                type: ProtocolConstants.MessagePickup3StatusRequest,
                body: body
            )
            .to(new List<string>() { request.MediatorDid })
            .build();

        var didComm = new DidComm(_didDocResolver, _secretResolver);

        // We pack the message and encrypt it for the mediator
        var packResult =await  didComm.PackEncrypted(
            new PackEncryptedParamsBuilder(statusRequestMessage, to: request.MediatorDid)
                .From(request.LocalDid)
                .ProtectSenderId(false)
                .BuildPackEncryptedParams()
        );

        // We send the message to the mediator
        HttpResponseMessage response;
        try
        {
            response = await _httpClient.PostAsync(request.MediatorEndpoint, new StringContent(packResult.PackedMessage, new MediaTypeHeaderValue(MessageTyp.Encrypted) ), cancellationToken);
        }
        catch (HttpRequestException ex)
        {
            return Result.Fail($"Connection could not be established: {ex.Message}");
        }
        
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return Result.Fail("Connection could not be established");
        }

        if (!response.IsSuccessStatusCode)
        {
            return Result.Fail("Unable to initiate connection: " + response.StatusCode);
        }

        var content = await response.Content.ReadAsStringAsync(cancellationToken);

        var unpackResult =await  didComm.Unpack(
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

                return Result.Ok(new StatusRequestResponse(problemReport.Value));
            }
            return Result.Fail("Error parsing the problem report of the mediator. Missing parent-thread-id");
        }
        
        if (unpackResult.Value.Message.Type != ProtocolConstants.MessagePickup3StatusResponse)
        {
            return Result.Fail($"Unexpected header-type: {unpackResult.Value.Message.Type}");
        }

        var bodyContent = unpackResult.Value.Message.Body;

        var statusRequestResponseResult = StatusRequestResponse.Parse(bodyContent);
        
        return statusRequestResponseResult;
    }
}