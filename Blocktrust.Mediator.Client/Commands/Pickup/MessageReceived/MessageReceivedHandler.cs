﻿namespace Blocktrust.Mediator.Client.Commands.Pickup.MessageReceived;

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

public class MessageReceivedHandler : IRequestHandler<MessageReceivedRequest, Result<StatusRequestResponse>>
{
    private readonly HttpClient _httpClient;
    private readonly IDidDocResolver _didDocResolver;
    private readonly ISecretResolver _secretResolver;

    public MessageReceivedHandler(HttpClient httpClient, IDidDocResolver didDocResolver, ISecretResolver secretResolver)
    {
        _httpClient = httpClient;
        _didDocResolver = didDocResolver;
        _secretResolver = secretResolver;
    }

    public async Task<Result<StatusRequestResponse>> Handle(MessageReceivedRequest request, CancellationToken cancellationToken)
    {
        var body = new Dictionary<string, object>();
        body.Add("message_id_list", request.MessageIds);

        var statusRequestMessage = new MessageBuilder(
                id: Guid.NewGuid().ToString(),
                type: ProtocolConstants.MessagePickup3MessagesReceived,
                body: body
            )
            .returnRoute("all")
            .to(new List<string>() { request.MediatorDid })
            .from(request.LocalDid)
            .build();

        var didComm = new DidComm(_didDocResolver, _secretResolver);

        // We pack the message and encrypt it for the mediator
        var packResult =await  didComm.PackEncrypted(
            new PackEncryptedParamsBuilder(statusRequestMessage, to: request.MediatorDid)
                .From(request.LocalDid)
                .ProtectSenderId(false)
                .BuildPackEncryptedParams()
        );

        if(packResult.IsFailed)
        {
            return packResult.ToResult();
        }
        
        // We send the message to the mediator
        HttpResponseMessage response;
        try
        {
            response = await _httpClient.PostAsync(request.MediatorEndpoint,new StringContent(packResult.Value.PackedMessage, new MediaTypeHeaderValue(MessageTyp.Encrypted) ), cancellationToken);
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

        if (String.IsNullOrEmpty(content))
        {
            // Usually that code should fail if we don't have any content coming back, but for compatibility reasons with the PRISM Mediator
            // we accept an empty response as a success
            
            return Result.Ok(new StatusRequestResponse());
        }
        
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