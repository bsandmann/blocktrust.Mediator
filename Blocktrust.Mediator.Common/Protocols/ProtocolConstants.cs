namespace Blocktrust.Mediator.Common.Protocols;

public class ProtocolConstants
{
    public const string OutOfBand2Invitation = "https://didcomm.org/out-of-band/2.0/invitation";

    public const string CoordinateMediation2Request = "https://didcomm.org/coordinate-mediation/2.0/mediate-request";
    public const string CoordinateMediation2Deny = "https://didcomm.org/coordinate-mediation/2.0/mediate-deny";
    public const string CoordinateMediation2Grant = "https://didcomm.org/coordinate-mediation/2.0/mediate-grant";
    public const string CoordinateMediation2KeylistUpdate = "https://didcomm.org/coordinate-mediation/2.0/keylist-update";
    public const string CoordinateMediation2KeylistUpdateResponse = "https://didcomm.org/coordinate-mediation/2.0/keylist-update-response";
    public const string CoordinateMediation2KeylistQuery = "https://didcomm.org/coordinate-mediation/2.0/keylist-query";
    public const string CoordinateMediation2KeylistQueryResponse = "https://didcomm.org/coordinate-mediation/2.0/keylist";

    public const string BasicMessage = "https://didcomm.org/basicmessage/2.0/message";
    public const string ForwardMessage = "https://didcomm.org/routing/2.0/forward";

    public const string MessagePickup3StatusRequest = "https://didcomm.org/messagepickup/3.0/status-request";
    public const string MessagePickup3StatusResponse = "https://didcomm.org/messagepickup/3.0/status";
    public const string MessagePickup3DeliveryRequest = "https://didcomm.org/messagepickup/3.0/delivery-request";
    public const string MessagePickup3DeliveryResponse = "https://didcomm.org/messagepickup/3.0/delivery";
    public const string MessagePickup3MessagesReceived = "https://didcomm.org/messagepickup/3.0/messages-received";
    public const string MessagePickup3LiveDeliveryChange = "https://didcomm.org/messagepickup/3.0/live-delivery-change";

    public const string ProblemReport = "https://didcomm.org/report-problem/2.0/problem-report";

    public const string DiscoverFeatures2Query = "https://didcomm.org/discover-features/2.0/queries";
    public const string DiscoverFeatures2Response = "https://didcomm.org/discover-features/2.0/disclose";

    public const string TrustPingRequest = "https://didcomm.org/trust-ping/2.0/ping";
    public const string TrustPingResponse = "https://didcomm.org/trust-ping/2.0/ping-response";
    
    public const string ShortenedUrlRequest = "https://didcomm.org/shorten-url/1.0/request-shortened-url";
    public const string ShortenedUrlResponse = "https://didcomm.org/shorten-url/1.0/shortened-url";
    public const string InvalidateShortenedUrl = "https://didcomm.org/shorten-url/1.0/invalidate-shortened-url";
}