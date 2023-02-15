﻿namespace Blocktrust.Mediator.Tests.TestData.Secrets;

using DIDComm_v2.Secrets;

public interface SecretResolverInMemoryMock : SecretResolver
{
    List<Secret> GetSecrets();
    List<string> GetSecretKids();
}