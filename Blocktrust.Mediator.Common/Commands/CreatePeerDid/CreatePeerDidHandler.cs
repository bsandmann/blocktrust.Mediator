namespace Blocktrust.Mediator.Server.Commands.CreatePeerDid;

using System.Text.Json;
using Server;
using Blocktrust.PeerDID.DIDDoc;
using Blocktrust.PeerDID.PeerDIDCreateResolve;
using Blocktrust.PeerDID.Types;
using Common.Commands.CreatePeerDid;
using FluentResults;
using MediatR;

/// <summary>
/// Handler to create new blocks inside the node-database to represent a block
/// </summary>
public class CreatePeerDidHandler : IRequestHandler<CreatePeerDidRequest, Result<PeerDid>>
{
    /// <summary>
    /// Handler
    /// </summary>
    /// <param name="createdidRequest"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<Result<PeerDid>> Handle(CreatePeerDidRequest createdidRequest, CancellationToken cancellationToken)
    {
        var encryptionKeys = new List<VerificationMaterialAgreement>();
        for (int i = 0; i < createdidRequest.NumberOfAgreementKeys; i++)
        {
            //TODO I should lose the private key??
            // Create 1 key pairs for aggreement keys (X25519)
            //  one public with crv, x, kty, kid
            // one private with crv, x, kty, kid, d
            var keypair = DIDComm_v2.Secrets.SecretUtils.GenerateX25519Keys();
            encryptionKeys.Add(
                new VerificationMaterialAgreement(
                    format: VerificationMaterialFormatPeerDid.Jwk,
                    type: VerificationMethodTypeAgreement.JsonWebKey2020,
                    value: keypair.PublicKey)
            );
        }

        var signingKeys = new List<VerificationMaterialAuthentication>();
        for (int i = 0; i < createdidRequest.NumberOfAuthenticationKeys; i++)
        {
            // Create 1 key pair for authentication (signing) ED25519
            // one public with crv, x, kty, kid
            // one private with crv, x, kty, kid, d
            var keypair = DIDComm_v2.Secrets.SecretUtils.GenerateEd25519Keys();
            signingKeys.Add(
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
            serviceDictionary = new PeerDidService(
                id: "new-id",
                type: Blocktrust.PeerDID.DIDDoc.ServiceConstants.ServiceDidcommMessaging,
                serviceEndpoint: createdidRequest.ServiceEndpoint,
                routingKeys: createdidRequest.ServiceRoutingKeys,
                accept: new List<string>() { "didcomm/v2" }).ToDict();

            service = JsonSerializer.Serialize(serviceDictionary);
        }


        // Generate Peer Did
        var peerDidString = string.Empty;
        if (encryptionKeys.Count == 1 && !signingKeys.Any() && serviceDictionary is null)
        {
            peerDidString = PeerDidCreator.CreatePeerDidNumalgo0((VerificationMaterialAuthentication)encryptionKeys.Single().Value);
        }
        else
        {
            peerDidString = PeerDidCreator.CreatePeerDidNumalgo2(encryptionKeys, signingKeys, service);
        }

        return Result.Ok(new PeerDid(peerDidString));
    }
}