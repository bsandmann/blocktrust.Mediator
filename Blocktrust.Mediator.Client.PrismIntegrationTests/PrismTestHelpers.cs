namespace Blocktrust.Mediator.Client.PrismIntegrationTests;

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
        var invitationResponse = await httpClient.PostAsync(prismAgentUrlRunningInDocker + "prism-agent/connection-invitations", invitationJson, new CancellationToken());
        if (!invitationResponse.IsSuccessStatusCode)
        {
            throw new Exception("Prism node not reachable");
        }
    }

    /// <summary>
    /// Sends a request to get all the existing connections of the PRISM agent.
    /// </summary>
    /// <param name="prismAgentApiKey"></param>
    /// <param name="prismAgentUrlRunningInDocker"></param>
    /// <param name="invitation"></param>
    /// <returns></returns>
    public static async Task<string> CreateUnpublishedDid(string prismAgentApiKey, string prismAgentUrlRunningInDocker)
    {
        using var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Add("apiKey", prismAgentApiKey);
        var innerContent =
            """{ "documentTemplate": { "publicKeys": [{ "id": "key-1", "purpose": "authentication"},{ "id": "key-2", "purpose": "keyAgreement"},{ "id": "key-3", "purpose": "assertionMethod"}], "services": [{ "id": "service-1", "type": "LinkedDomains", "serviceEndpoint": [ "https://bar.example.com/"]}]}}""";
        var invitationJson = new StringContent(innerContent, Encoding.UTF8, "application/json");
        var unpublishedDidResponse = await httpClient.PostAsync(prismAgentUrlRunningInDocker + "prism-agent/did-registrar/dids", invitationJson, new CancellationToken());
        var unpublishedDidContent = await unpublishedDidResponse.Content.ReadAsStringAsync();
        var jsonDocument = JsonDocument.Parse(unpublishedDidContent);
        jsonDocument.RootElement.TryGetProperty("longFormDid", out var longFormDid);
        return longFormDid.GetString();
    }

    /// Sends a request to get all the existing connections of the PRISM agent.
    /// </summary>
    /// <param name="prismAgentApiKey"></param>
    /// <param name="prismAgentUrlRunningInDocker"></param>
    /// <param name="invitation"></param>
    /// <returns></returns>
    public static async Task<string> PublishDid(string prismAgentApiKey, string prismAgentUrlRunningInDocker, string didToPublish)
    {
        using var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Add("apiKey", prismAgentApiKey);
        var innerContent = "";
        var invitationJson = new StringContent(innerContent, Encoding.UTF8, "application/json");
        var unpublishedDidResponse = await httpClient.PostAsync(prismAgentUrlRunningInDocker + $"prism-agent/did-registrar/dids/{didToPublish}/publications", invitationJson, new CancellationToken());
        var unpublishedDidContent = await unpublishedDidResponse.Content.ReadAsStringAsync();
        var jsonDocument = JsonDocument.Parse(unpublishedDidContent);
        jsonDocument.RootElement.TryGetProperty("scheduledOperation", out var ops);
        var opsJson = ops.EnumerateObject();
        var opsId = "";
        foreach (var opsProperty in opsJson)
        {
            if (opsProperty.NameEquals("id"))
            {
                opsId = opsProperty.Value.GetString();
            }
        }

        return opsId;
    }

    /// Sends a request to get all the existing connections of the PRISM agent.
    /// </summary>
    /// <param name="prismAgentApiKey"></param>
    /// <param name="prismAgentUrlRunningInDocker"></param>
    /// <param name="invitation"></param>
    /// <returns></returns>
    public static async Task<bool> CreateCredentialOffer(string prismAgentApiKey, string prismAgentUrlRunningInDocker, string issuingDid, string connectionId)
    {
        using var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Add("apiKey", prismAgentApiKey);
        var innerContent1 = """{"validityPeriod": 3600, "claims": { "name1": "value1", "name2": "value2"}, "automaticIssuance": true, "issuingDID":""";
        var innerContent2 = $"\"{issuingDid}\",\"connectionId\":\"{connectionId}\"";
        var innerContent3 = "}";
        var innerContent = string.Concat(innerContent1, innerContent2, innerContent3);
        var invitationJson = new StringContent(innerContent, Encoding.UTF8, "application/json");
        var credentialOfferRepsonse = await httpClient.PostAsync(prismAgentUrlRunningInDocker + $"prism-agent/issue-credentials/credential-offers", invitationJson, new CancellationToken());
        var unpublishedDidContent = await credentialOfferRepsonse.Content.ReadAsStringAsync();
        var jsonDocument = JsonDocument.Parse(unpublishedDidContent);

        jsonDocument.RootElement.TryGetProperty("protocolState", out var protocolState);
        var state = protocolState.GetString();
        if (state.Equals("OfferPending", StringComparison.InvariantCultureIgnoreCase))
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// Sends a request to get all the existing connections of the PRISM agent.
    /// </summary>
    /// <param name="prismAgentApiKey"></param>
    /// <param name="prismAgentUrlRunningInDocker"></param>
    /// <returns></returns>
    public static async Task<List<(string, string)>> GetDids(string prismAgentApiKey, string prismAgentUrlRunningInDocker)
    {
        using var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Add("apiKey", prismAgentApiKey);
        var getConnections = await httpClient.GetAsync(prismAgentUrlRunningInDocker + "prism-agent/did-registrar/dids", new CancellationToken());
        var connectionFromPrismAgent = await getConnections.Content.ReadAsStringAsync();
        var jsonDocument = JsonDocument.Parse(connectionFromPrismAgent);
        jsonDocument.RootElement.TryGetProperty("contents", out var contents);
        var dids = new List<(string, string)>();
        if (contents.EnumerateArray().Count() >= 100)
        {
            // We have to implement pagination
            throw new NotImplementedException();
        }

        foreach (var didJson in contents.EnumerateArray())
        {
            didJson.TryGetProperty("did", out var did);
            var didString = did.GetString();
            didJson.TryGetProperty("status", out var status);
            var statusString = status.GetString();
            dids.Add((didString, statusString));
        }

        return dids;
    }
    
    /// <summary>
    /// Sends a request to get all the existing connections of the PRISM agent.
    /// </summary>
    /// <param name="prismAgentApiKey"></param>
    /// <param name="prismAgentUrlRunningInDocker"></param>
    /// <returns></returns>
    public static async Task<string> GetDid(string prismAgentApiKey, string prismAgentUrlRunningInDocker, string did)
    {
        using var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Add("apiKey", prismAgentApiKey);
        var getConnections = await httpClient.GetAsync(prismAgentUrlRunningInDocker + $"prism-agent/did-registrar/dids/{did}", new CancellationToken());
        var connectionFromPrismAgent = await getConnections.Content.ReadAsStringAsync();
        var jsonDocument = JsonDocument.Parse(connectionFromPrismAgent);
        jsonDocument.RootElement.TryGetProperty("status", out var status);
        return status.GetString();
    }
}