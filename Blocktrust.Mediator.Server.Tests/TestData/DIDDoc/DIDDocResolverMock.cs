namespace Blocktrust.Mediator.Server.Tests.TestData.DIDDoc;

using DIDComm_v2.DidDocs;

public class DIDDocResolverMock : DIDDocResolver
{
    private DidDocResolverInMemory didDocResolver;

    public DIDDocResolverMock()
    {
        didDocResolver = new DidDocResolverInMemory(new List<DidDoc>()
        {
            DIDDocAlice.DID_DOC_ALICE_SPEC_TEST_VECTORS,
            DIDDocBob.DID_DOC_BOB_SPEC_TEST_VECTORS,
            DIDDocRootsMediator.DID_DOC_ROOTS_SPEC_TEST_VECTORS,
        });
    }

    public DidDoc? Resolve(String did)
    {
        return didDocResolver.Resolve(did);
    }
}