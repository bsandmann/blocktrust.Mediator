namespace Blocktrust.Mediator.Client.Commands.PrismConnect.AnwserConnectRequest;

using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Blocktrust.Common.Converter;
using Blocktrust.Common.Models.DidDoc;
using Blocktrust.Common.Resolver;
using Common;
using Common.Models.ProblemReport;
using Common.Protocols;
using CredentialRequest;
using DIDComm;
using DIDComm.Common.Types;
using DIDComm.Message.Attachments;
using DIDComm.Message.Messages;
using DIDComm.Model.PackEncryptedParamsModels;
using FluentResults;
using MediatR;
using Pickup.DeliveryRequest;
using Pickup.MessageReceived;
using ProcessOobInvitationAndConnect;

public class CredentialRequestHandler : IRequestHandler<CredentialRequestRequest, Result<CredentialRequestResponse>>
{
    private readonly HttpClient _httpClient;
    private readonly IDidDocResolver _didDocResolver;
    private readonly ISecretResolver _secretResolver;

    public CredentialRequestHandler(HttpClient httpClient, IDidDocResolver didDocResolver, ISecretResolver secretResolver)
    {
        _httpClient = httpClient;
        _didDocResolver = didDocResolver;
        _secretResolver = secretResolver;
    }


    public async Task<Result<CredentialRequestResponse>> Handle(CredentialRequestRequest request, CancellationToken cancellationToken)
    {
        var body = new Dictionary<string, object>();
        body.Add("goal_code", GoalCodes.PrismCredentialOffer);
        body.Add("comment", null);
        body.Add("formats", new List<string>());
        var attachment = new AttachmentBuilder(Guid.NewGuid().ToString(), new Base64(request.SignedJwtCredentialRequest)).Build();
        var mediateRequestMessage = new MessageBuilder(
                id: Guid.NewGuid().ToString(),
                type: ProtocolConstants.IssueCredential2Request,
                body: body
            )
            .thid(request.MessageId)
            .from(request.LocalPeerDid.Value)
            .to(new List<string>() { request.PrismPeerDid.Value })
            .attachments(new List<Attachment>()
            {
                attachment
            })
            .build();

        var didComm = new DidComm(_didDocResolver, _secretResolver);
        var packResult = await didComm.PackEncrypted(
            new PackEncryptedParamsBuilder(mediateRequestMessage, to: request.PrismPeerDid.Value)
                .From(request.LocalPeerDid.Value)
                .ProtectSenderId(false)
                .BuildPackEncryptedParams()
        );

        if (packResult.IsFailed)
        {
            return packResult.ToResult();
        }

        var prismDidDoc = await _didDocResolver.Resolve(request.PrismPeerDid.Value);
        if (prismDidDoc is null)
        {
            return Result.Fail("Error resolving prism did");
        }

        var prismAgentEndpoint = prismDidDoc.Services.FirstOrDefault().ServiceEndpoint;
        if (string.IsNullOrEmpty(prismAgentEndpoint.Uri))
        {
            return Result.Fail("Error parsing the endpoint of the prism did");
        }

#if DEBUG
        // The problem is that the prism agent is running in a docker container and the mediator is running on the host machine.
        // For more details read the documentation in the test for this method
        prismAgentEndpoint = new ServiceEndpoint(uri: prismAgentEndpoint.Uri.Replace("host.docker.internal", "localhost"));
#endif

        var prismDidEndpointUri = new Uri(prismAgentEndpoint.Uri);

        // We send the message to agent
        HttpResponseMessage response;
        try
        {
            response = await _httpClient.PostAsync(prismDidEndpointUri, new StringContent(packResult.Value.PackedMessage, new MediaTypeHeaderValue(MessageTyp.Encrypted)), cancellationToken);
        }
        catch (HttpRequestException ex)
        {
            return Result.Fail($"Connection could not be established: {ex.Message}");
        }

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return Result.Fail("Connection could not be established. Not found.");
        }

        if (!response.IsSuccessStatusCode)
        {
            return Result.Fail("Unable to initiate connection: " + response.StatusCode);
        }

        return new CredentialRequestResponse();
    }
}