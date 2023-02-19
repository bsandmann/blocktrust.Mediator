namespace Blocktrust.Mediator.Server.Tests.TestData.Secrets;

using Blocktrust.Common.Models.Secrets;
using DIDComm_v2.Secrets;

public interface SecretResolverInMemoryMock : ISecretResolver
{
    List<Secret> GetSecrets();
    List<string> GetSecretKids();
}