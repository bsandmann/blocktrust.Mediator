namespace Blocktrust.Mediator.Common;

public static class GoalCodes
{
    
    /// <summary>
    /// Used for the initial request for mediation
    /// Attention: In the first version of the mediator, a underscore was used instead!
    /// </summary>
    public readonly static string RequestMediation = "request-mediation";
    
    /// <summary>
    /// Used to shorten a url
    /// </summary>
    public readonly static string ShortenOobv2 = "shorten.oobv2";
    
    /// <summary>
    /// Used to shorten a url
    /// </summary>
    public readonly static string PrismConnect = "Connect";
}