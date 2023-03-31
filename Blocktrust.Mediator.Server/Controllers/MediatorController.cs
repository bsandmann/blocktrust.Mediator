namespace Blocktrust.Mediator.Server.Controllers;

using System.Text;
using System.Text.Json;
using Blocktrust.Common.Resolver;
using Commands.ProcessMessage;
using DIDComm;
using DIDComm.Common.Types;
using DIDComm.Model.PackEncryptedParamsModels;
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
        if (System.Diagnostics.Debugger.IsAttached && hostUrl.Equals("http://localhost:5023"))
        {
            // This is only for local development and testing the mediator with a PRISM agent running in a docker container
            hostUrl = "http://host.docker.internal:5023";
        }

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


        string? senderOldDid = null;
        string? senderDid = null;

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
                // Should only be really the case for forward messages ?!
                senderDid = null;
                senderOldDid = null;
            }
            else
            {
                var split = encryptedFrom.Split("#");
                senderDid = split.First();
                senderOldDid = senderDid;
            }
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
            if (processMessageResponse.RespondWithAccepted || senderDid is null || processMessageResponse.MediatorDid is null)
            {
                return Accepted();
            }

            var packResult = await didComm.PackEncrypted(
                new PackEncryptedParamsBuilder(processMessageResponse.Message, to: senderDid)
                    .From(processMessageResponse.MediatorDid)
                    .ProtectSenderId(false)
                    .BuildPackEncryptedParams()
            );
            
            if(packResult.IsFailed)
            {
                return BadRequest($"Unable to pack message: {packResult.Errors.First().Message}");
            }

            return Ok(packResult.Value.PackedMessage);
        }
        else
        {
            if (processMessageResponse.RespondWithAccepted || senderDid is null || processMessageResponse.MediatorDid is null)
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
            
            if(packResult.IsFailed)
            {
                return BadRequest($"Unable to pack message: {packResult.Errors.First().Message}");
            }

            var didDocSenderDid = await _didDocResolver.Resolve(senderDid);
            if (didDocSenderDid is null)
            {
                return Ok(packResult.Value.PackedMessage);
            }

            var service = didDocSenderDid.Services.FirstOrDefault();
            if (service is null)
            {
                // Fallback to just sending a http-response
                return Ok(packResult.Value.PackedMessage);
            }

            var endpoint = service!.ServiceEndpoint;
            if (string.IsNullOrEmpty(endpoint))
            {
                // Fallback to just sending a http-response
                return Ok(packResult.Value.PackedMessage);
            }

            var isUri = Uri.TryCreate(endpoint, UriKind.Absolute, out var endpointUri);
            if (!isUri)
            {
                // Fallback to just sending a http-response
                return Ok(packResult.Value.PackedMessage);
            }

            var response = await _httpClient.PostAsync(endpointUri, new StringContent(packResult.Value.PackedMessage, Encoding.UTF8, MessageTyp.Encrypted));
            if (!response.IsSuccessStatusCode)
            {
                // Fallback to just sending a http-response
                return Ok(packResult.Value.PackedMessage);
            }

            return BadRequest($"Error sending message back to: '{senderDid}");
        }
    }
}