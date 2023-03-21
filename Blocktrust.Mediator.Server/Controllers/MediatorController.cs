namespace Blocktrust.Mediator.Server.Controllers;

using System.Text;
using System.Text.Json;
using Blocktrust.Common.Resolver;
using Commands.DatabaseCommands.CreateOobInvitation;
using Commands.DatabaseCommands.GetOobInvitation;
using Commands.ProcessMessage;
using Common.Commands.CreatePeerDid;
using DIDComm;
using DIDComm.Common.Types;
using DIDComm.Message.Messages;
using DIDComm.Model.PackEncryptedParamsModels;
using DIDComm.Model.PackEncryptedResultModels;
using DIDComm.Model.UnpackParamsModels;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Models;

[ApiController]
public class MediatorController : ControllerBase
{
    private readonly ILogger<MediatorController> _logger;
    private readonly HttpClient _httpClient;
    private readonly IMediator _mediator;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ISecretResolver _secretResolver;
    private readonly IDidDocResolver _didDocResolver;

    public MediatorController(ILogger<MediatorController> logger, IMediator mediator, IHttpContextAccessor httpContextAccessor, ISecretResolver secretResolver, IDidDocResolver didDocResolver, HttpClient httpClient)
    {
        _logger = logger;
        _mediator = mediator;
        _httpContextAccessor = httpContextAccessor;
        _secretResolver = secretResolver;
        _didDocResolver = didDocResolver;
        _httpClient = httpClient;
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
        var unpacked = await didComm.Unpack(
            new UnpackParamsBuilder(body).BuildUnpackParams()
        );
        if (unpacked.IsFailed)
        {
            return BadRequest($"Unable to unpack message: {unpacked.Errors.First().Message}");
        }

        string senderOldDid;
        string senderDid;
        if (unpacked.Value.Message.FromPrior is not null)
        {
            //TODO assertions? 
            senderOldDid = unpacked.Value.Message.FromPrior.Iss;
            senderDid = unpacked.Value.Message.FromPrior.Sub;
        }
        else
        {
            var encryptedFrom = unpacked.Value.Metadata.EncryptedFrom;
            if (encryptedFrom is null)
            {
                return BadRequest("Unable to unpack message: Unable to read encryptedFrom in Metadata");
            }

            var split = encryptedFrom.Split("#");
            senderDid = split.First();
            senderOldDid = senderDid;
        }

        var processMessageResponse = await _mediator.Send(new ProcessMessageRequest(senderOldDid, senderDid, hostUrl, unpacked.Value));

        // Check if we have a return route flag. Otherwise we should send a separate message
        var customHeaders = unpacked.Value.Message.CustomHeaders;
        EnumReturnRoute returnRoute = EnumReturnRoute.None;
        if ((customHeaders.TryGetValue("return_route", out var returnRouteString)))
        {
            var returnRouteJsonElement = (JsonElement)returnRouteString;
            if (returnRouteJsonElement.ValueKind == JsonValueKind.String)
            {
                Enum.TryParse(returnRouteJsonElement.GetString(), true, out returnRoute);
            }
        }

        //TODO in some cases I might want to respond with a empty-message instead or accepted or a defined response. Figure out where

        // TODO simplification: the correct use of 'thid' here should be tested 
        if (returnRoute == EnumReturnRoute.All || (returnRoute == EnumReturnRoute.Thread && processMessageResponse.Message.Thid is not null && processMessageResponse.Message!.Thid!.Equals(unpacked.Value.Message.Thid)))
        {
            if (processMessageResponse.RespondWithAccepted)
            {
                return Accepted();
            }

            var packResult = await didComm.PackEncrypted(
                new PackEncryptedParamsBuilder(processMessageResponse.Message, to: senderDid)
                    .From(processMessageResponse.MediatorDid)
                    .ProtectSenderId(false)
                    .BuildPackEncryptedParams()
            );

            return Ok(packResult.PackedMessage);
        }
        else
        {
            if (processMessageResponse.RespondWithAccepted)
            {
                //TODO I should fall back to an empty message here

                return Accepted();
            }

            var packResult = await didComm.PackEncrypted(
                new PackEncryptedParamsBuilder(processMessageResponse.Message, to: senderDid)
                    .From(processMessageResponse.MediatorDid)
                    .ProtectSenderId(false)
                    .BuildPackEncryptedParams()
            );

            var didDocSenderDid = await _didDocResolver.Resolve(senderDid);
            if (didDocSenderDid is null)
            {
                return Ok(packResult.PackedMessage);
            }
            
            var service = didDocSenderDid.Services.FirstOrDefault();
            if (service is null)
            {
                // Fallback to just sending a http-response
                return Ok(packResult.PackedMessage);
            }

            var endpoint = service!.ServiceEndpoint;
            if (string.IsNullOrEmpty(endpoint))
            {
                // Fallback to just sending a http-response
                return Ok(packResult.PackedMessage);
            }

            var isUri = Uri.TryCreate(endpoint, UriKind.Absolute, out var endpointUri);
            if (!isUri)
            {
                // Fallback to just sending a http-response
                return Ok(packResult.PackedMessage);
            }

            var response = await _httpClient.PostAsync(endpointUri, new StringContent(packResult.PackedMessage, Encoding.UTF8, MessageTyp.Encrypted));
            if (!response.IsSuccessStatusCode)
            {
                // Fallback to just sending a http-response
                return Ok(packResult.PackedMessage);
            }

            return BadRequest($"Error sending message back to: '{senderDid}");
        }
    }
}