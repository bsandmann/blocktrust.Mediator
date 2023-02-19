namespace Blocktrust.Mediator.Server.Tests.TestData.Secrets;

using Blocktrust.Common.Models.Secrets;
using Blocktrust.Common.Resolver;
using DIDComm_v2.Secrets;

public interface SecretResolverInMemoryMock : ISecretResolver
{
    List<Secret> GetSecrets();
    List<string> GetSecretKids();
}