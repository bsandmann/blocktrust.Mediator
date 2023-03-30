namespace Blocktrust.Mediator.Client.PrismIntegrationTests;

using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

public static class PrismTestHelpers
{
    /// <summary>
    /// Sends a request to the PRISM agent to get a OOB invitation.
    /// </summary>
    /// <param name="prismAgentApiKey"></param>
    /// <param name="prismAgentUrlRunningInDocker"></param>
    /// <returns></returns>
    public static async Task<string> RequestOutOfBandInvitation(string prismAgentApiKey, string prismAgentUrlRunningInDocker)
    {
        using var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Add("apiKey", prismAgentApiKey);
        var getOOBInvitationFromPrismAgent = await httpClient.PostAsync(prismAgentUrlRunningInDocker + "prism-agent/connections", new StringContent("""{"label":"test"}""", Encoding.UTF8, "application/json"), new CancellationToken());
        var oobInvitationFromPrismAgent = await getOOBInvitationFromPrismAgent.Content.ReadAsStringAsync();
        var jsonDocument = JsonDocument.Parse(oobInvitationFromPrismAgent);
        jsonDocument.RootElement.TryGetProperty("invitation", out var invitation);
        invitation.TryGetProperty("invitationUrl", out var invitationUrl);
        var invitationUrlString = invitationUrl.GetString();
        var prismOob = invitationUrlString.Split('=');
        return prismOob[1];
    }

    /// <summary>
    /// Sends a request to get all the existing connections of the PRISM agent.
    /// </summary>
    /// <param name="prismAgentApiKey"></param>
    /// <param name="prismAgentUrlRunningInDocker"></param>
    /// <returns></returns>
    public static async Task<List<string>> GetConnections(string prismAgentApiKey, string prismAgentUrlRunningInDocker)
    {
        using var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Add("apiKey", prismAgentApiKey);
        var getConnections = await httpClient.GetAsync(prismAgentUrlRunningInDocker + "prism-agent/connections", new CancellationToken());
        var connectionFromPrismAgent = await getConnections.Content.ReadAsStringAsync();
        var jsonDocument = JsonDocument.Parse(connectionFromPrismAgent);
        jsonDocument.RootElement.TryGetProperty("contents", out var contents);
        var connectionIds = new List<string>();
        foreach (var connectionJson in contents.EnumerateArray())
        {
            connectionJson.TryGetProperty("connectionId", out var connectionId);
            var connectionIdString = connectionId.GetString();
            connectionIds.Add(connectionIdString);
        }

        return connectionIds;
    }

    /// <summary>
    /// Sends a request to get all the existing connections of the PRISM agent.
    /// </summary>
    /// <param name="prismAgentApiKey"></param>
    /// <param name="prismAgentUrlRunningInDocker"></param>
    /// <param name="invitation"></param>
    /// <returns></returns>
    public static async Task SendInvitation(string prismAgentApiKey, string prismAgentUrlRunningInDocker, string invitation)
    {
        using var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Add("apiKey", prismAgentApiKey);
        var innerContent = string.Concat("{\"invitation\":\"", invitation, "\"}"); 
        var invitationJson = new StringContent(innerContent, Encoding.UTF8, "application/json");
        var invitationResponse = await httpClient.PostAsync(prismAgentUrlRunningInDocker + "prism-agent/connection-invitations", invitationJson ,new CancellationToken());
        if (!invitationResponse.IsSuccessStatusCode)
        {
            throw new Exception("Prism node not reachable");
        }
    }
}