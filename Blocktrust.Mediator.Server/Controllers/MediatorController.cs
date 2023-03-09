﻿namespace Blocktrust.Mediator.Server.Controllers;

using System.Text.Json;
using Blocktrust.Common.Resolver;
using Commands.DatabaseCommands.CreateOobInvitation;
using Commands.DatabaseCommands.GetOobInvitation;
using Commands.ProcessMessage;
using Common.Commands.CreatePeerDid;
using DIDComm;
using DIDComm.Model.PackEncryptedParamsModels;
using DIDComm.Model.UnpackParamsModels;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Models;

[ApiController]
public class MediatorController : ControllerBase
{
    private readonly ILogger<MediatorController> _logger;
    private readonly IMediator _mediator;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ISecretResolver _secretResolver;
    private readonly IDidDocResolver _didDocResolver;

    public MediatorController(ILogger<MediatorController> logger, IMediator mediator, IHttpContextAccessor httpContextAccessor, ISecretResolver secretResolver, IDidDocResolver didDocResolver)
    {
        _logger = logger;
        _mediator = mediator;
        _httpContextAccessor = httpContextAccessor;
        _secretResolver = secretResolver;
        _didDocResolver = didDocResolver;
    }

    /// <summary>
    /// Mediator endpoint
    /// </summary>
    /// <returns></returns>
    [HttpPost("/")]
    public async Task<ActionResult<string>> Mediate()
    {
        var hostUrl = string.Concat(_httpContextAccessor!.HttpContext.Request.Scheme, "://", _httpContextAccessor.HttpContext.Request.Host);
        var request = _httpContextAccessor.HttpContext.Request;
        var body = await new StreamReader(request.Body).ReadToEndAsync();

        var didComm = new DidComm(_didDocResolver, _secretResolver);
        var unpacked = didComm.Unpack(
            new UnpackParamsBuilder(body).BuildUnpackParams()
        );
        if (unpacked.IsFailed)
        {
            return BadRequest("Unable to unpack message");
        }

        string senderOldDid;
        string senderDid;
        if (unpacked.Value.Message.FromPrior is not null)
        {
            //TODO assertiongs that thease are not emtpy??
            senderOldDid = unpacked.Value.Message.FromPrior.Iss;
            senderDid = unpacked.Value.Message.FromPrior.Sub;
        }
        else
        {
            senderDid = unpacked.Value.Metadata.EncryptedFrom.Split("#")[0];
            senderOldDid = senderDid;
        }

        var processMessageResponse = await _mediator.Send(new ProcessMessageRequest(senderOldDid, senderDid, hostUrl, unpacked.Value));

        // Check if we have a return route flag. Otherwise we should send a separate message
        var customHeaders = unpacked.Value.Message.CustomHeaders;
        EnumReturnRoute returnRoute = EnumReturnRoute.None;
        if (customHeaders is not null && (customHeaders.TryGetValue("return_route", out var returnRouteString)))
        {
            var returnRouteJsonElement = (JsonElement)returnRouteString;
            if (returnRouteJsonElement.ValueKind == JsonValueKind.String)
            {
                EnumReturnRoute.TryParse(returnRouteJsonElement.GetString(), true, out returnRoute);
            }
        }

        // TODO simplification: we currently treat 'all' and 'thread' the same
        if (returnRoute == EnumReturnRoute.All || returnRoute == EnumReturnRoute.Thread)
        {
            if (processMessageResponse.RespondWithAccepted)
            {
                return Accepted();
            }

            var packResult = didComm.PackEncrypted(
                new PackEncryptedParamsBuilder(processMessageResponse.Message, to: senderDid)
                    .From(processMessageResponse.MediatorDid)
                    .ProtectSenderId(false)
                    .BuildPackEncryptedParams()
            );

            return Ok(packResult.PackedMessage);
        }
        else
        {
            // TODO we should queue the messages and send them out separately
            // TODO but we have to ensure that the sending endpoint has indeed a mediator or is a cloud agent
            if (processMessageResponse.RespondWithAccepted)
            {
                return Accepted();
            }

            var packResult = didComm.PackEncrypted(
                new PackEncryptedParamsBuilder(processMessageResponse.Message, to: senderDid)
                    .From(processMessageResponse.MediatorDid)
                    .ProtectSenderId(false)
                    .BuildPackEncryptedParams()
            );

            return Ok(packResult.PackedMessage);
        }
    }
}