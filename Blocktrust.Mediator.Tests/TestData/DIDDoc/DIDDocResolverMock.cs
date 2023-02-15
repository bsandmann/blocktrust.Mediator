namespace Blocktrust.Mediator.Tests.TestData.DIDDoc;

using DIDComm_v2.DidDocs;

public class DIDDocResolverMock : DIDDocResolver
{
    private DIDDocResolverInMemory didDocResolver;

    public DIDDocResolverMock()
    {
        didDocResolver = new DIDDocResolverInMemory(new List<DIDDoc>()
        {
            DIDDocAlice.DID_DOC_ALICE_SPEC_TEST_VECTORS,
            DIDDocBob.DID_DOC_BOB_SPEC_TEST_VECTORS,
            DIDDocRootsMediator.DID_DOC_ROOTS_SPEC_TEST_VECTORS,
        });
    }

    public DIDDoc? resolve(String did)
    {
        return didDocResolver.resolve(did);
    }
}