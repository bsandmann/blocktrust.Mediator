namespace Blocktrust.Mediator.Server.Commands.DiscoverFeatures;

using Blocktrust.DIDComm.Message.Messages;
using Blocktrust.Mediator.Common.Models.DiscoverFeatures;
using Blocktrust.Mediator.Common.Protocols;
using MediatR;

public class ProcessDiscoverFeaturesHandler : IRequestHandler<ProcessDiscoverFeaturesRequest, Message?>
{

    private List<DiscoverFeature> supportedProtocols = new List<DiscoverFeature>()
    {
        new DiscoverFeature("protocol", "https://didcomm.org/out-of-band/2.0"),
        new DiscoverFeature("protocol", "https://didcomm.org/coordinate-mediation/2.0"),
        new DiscoverFeature("protocol", "https://didcomm.org/messagepickup/3.0"),
        new DiscoverFeature("protocol", "https://didcomm.org/basicmessage/2.0"),
        new DiscoverFeature("protocol", "https://didcomm.org/trust-ping/2.0"),
        new DiscoverFeature("protocol", "https://didcomm.org/discover-features/2.0")
    };

    /// <inheritdoc />
    public async Task<Message?> Handle(ProcessDiscoverFeaturesRequest request, CancellationToken cancellationToken)
    {
        var body = request.UnpackedMessage.Body;
        var featureQueries = FeatureQuery.Parse(body);
        var disclosures = new List<DiscoverFeature>();

        foreach (var featureQuery in featureQueries)
        {
            if (featureQuery.FeatureType.Equals("protocol", StringComparison.InvariantCultureIgnoreCase))
            {
                if (featureQuery.Match.Equals("*"))
                {
                    disclosures.AddRange(supportedProtocols);
                }
                else
                {
                    disclosures.AddRange(supportedProtocols.Where(p => p.Id.StartsWith(featureQuery.Match.Replace(".*", ""))).ToList());
                }
            }
        }

        var distinctDisclosures = disclosures.DistinctBy(p => p.Id).ToList();

        var returnBody = new Dictionary<string, object>();
        returnBody.Add("disclosures", distinctDisclosures);
        var disclosureMessage = new MessageBuilder(
                id: Guid.NewGuid().ToString(),
                type: ProtocolConstants.DiscoverFeatures2Response,
                body: returnBody
            )
            .thid(request.UnpackedMessage.Thid ?? request.UnpackedMessage.Id)
            .fromPrior(request.FromPrior)
            .build();

        return disclosureMessage;
    }
}