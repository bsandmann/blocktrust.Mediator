namespace Blocktrust.Mediator.Common.Commands.CreatePeerDid;

using System.Text.Json;
using Blocktrust.Common.Models.DidDoc;
using Blocktrust.Common.Models.Secrets;
using Blocktrust.Common.Resolver;
using Blocktrust.PeerDID.DIDDoc;
using Blocktrust.PeerDID.PeerDIDCreateResolve;
using Blocktrust.PeerDID.Types;
using DIDComm.Secrets;
using FluentResults;
using MediatR;

/// <summary>
/// Handler to create new blocks inside the node-database to represent a block
/// </summary>
public class CreatePeerDidHandler : IRequestHandler<CreatePeerDidRequest, Result<CreatePeerDidResponse>>
{
    private readonly ISecretResolver _secretResolver;

    public CreatePeerDidHandler(ISecretResolver secretResolver)
    {
        this._secretResolver = secretResolver;
    }

    /// <summary>
    /// Handler
    /// </summary>
    /// <param name="createdidRequest"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<Result<CreatePeerDidResponse>> Handle(CreatePeerDidRequest createdidRequest, CancellationToken cancellationToken)
    {
        var publicAgreementKeys = new List<VerificationMaterialAgreement>();
        var privateAgreementKeys = new List<Secret>();
        for (int i = 0; i < createdidRequest.NumberOfAgreementKeys; i++)
        {
            // Create 1 key pairs for agreement keys (X25519)
            // one public with crv, x, kty, kid
            // one private with crv, x, kty, kid, d
            var keypair = SecretUtils.GenerateX25519Keys();
            privateAgreementKeys.Add(SecretUtils.JwkToSecret(keypair.PrivateKey));
            publicAgreementKeys.Add(
                new VerificationMaterialAgreement(
                    format: VerificationMaterialFormatPeerDid.Jwk,
                    type: VerificationMethodTypeAgreement.JsonWebKey2020,
                    value: keypair.PublicKey)
            );
        }

        var publicAuthenticationKeys = new List<VerificationMaterialAuthentication>();
        var privateAuthenticationKeys = new List<Secret>();
        for (int i = 0; i < createdidRequest.NumberOfAuthenticationKeys; i++)
        {
            // Create 1 key pair for authentication (signing) ED25519
            // one public with crv, x, kty, kid
            // one private with crv, x, kty, kid, d
            var keypair = SecretUtils.GenerateEd25519Keys();
            privateAuthenticationKeys.Add(SecretUtils.JwkToSecret(keypair.PrivateKey));
            publicAuthenticationKeys.Add(
                new VerificationMaterialAuthentication(
                    format: VerificationMaterialFormatPeerDid.Jwk,
                    type: VerificationMethodTypeAuthentication.JsonWebKey2020,
                    value: keypair.PublicKey)
            );
        }

        Dictionary<string, object>? serviceDictionary = null;
        string? service = null;

        if (!string.IsNullOrWhiteSpace(createdidRequest.ServiceEndpoint))
        {
            serviceDictionary = new Service(
                id: "new-id",
                serviceEndpoint: createdidRequest.ServiceEndpoint,
                routingKeys: createdidRequest.ServiceRoutingKeys,
                accept: new List<string>() { "didcomm/v2" },
                type: ServiceConstants.ServiceDidcommMessaging).ToDict();

            service = JsonSerializer.Serialize(serviceDictionary);
        }

        // Generate Peer Did
        string peerDidString;
        if (publicAgreementKeys.Count == 1 && !publicAuthenticationKeys.Any() && serviceDictionary is null)
        {
            peerDidString = PeerDidCreator.CreatePeerDidNumalgo0((VerificationMaterialAuthentication)publicAgreementKeys.Single().Value);
        }
        else
        {
            peerDidString = PeerDidCreator.CreatePeerDidNumalgo2(publicAgreementKeys, publicAuthenticationKeys, service);
        }

        var peerDidResult = PeerDidResolver.ResolvePeerDid(new PeerDid(peerDidString), VerificationMaterialFormatPeerDid.Jwk);
        if (peerDidResult.IsFailed)
        {
            throw new Exception("A PeerDID just created should always be resolvable.");
        }

        var peerDidDocResult = DidDocPeerDid.FromJson(peerDidResult.Value);
        if (peerDidDocResult.IsFailed)
        {
            throw new Exception("A PeerDID just created should always be resolvable.");
        }
        
        // Register the secrets of the created Did in the secretResolver
        
        var zippedAgreementKeysAndSecrets = privateAgreementKeys
            .Zip(peerDidDocResult.Value.KeyAgreements
                .Select(p => p.Id), (secret, kid) => new { secret = secret, kid = kid });
        foreach (var zip in zippedAgreementKeysAndSecrets)
        {
            zip.secret.Kid = zip.kid;
            _secretResolver.AddKey(zip.kid, zip.secret);
        }
        
        var zippedAuthenticationKeysAndSecrets = privateAuthenticationKeys
            .Zip(peerDidDocResult.Value.Authentications
                .Select(p => p.Id), (secret, kid) => new { secret = secret, kid = kid });
        foreach (var zip in zippedAuthenticationKeysAndSecrets)
        {
            zip.secret.Kid = zip.kid;
            _secretResolver.AddKey(zip.kid, zip.secret);
        }

        var response = new CreatePeerDidResponse(
            peerDid: new PeerDid(peerDidString),
            didDoc: peerDidDocResult.Value,
            privateAgreementKeys: privateAgreementKeys,
            privateAuthenticationKeys: privateAuthenticationKeys);
        
        return Result.Ok(response);
    }
}