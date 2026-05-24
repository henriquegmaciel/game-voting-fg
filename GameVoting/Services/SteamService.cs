using GameVoting.Services.Interfaces;
using System.Text.Json;

namespace GameVoting.Services;

public class SteamService : ISteamService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly string _groupId;

    public SteamService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _apiKey = configuration["Steam:ApiKey"]!;
        _groupId = configuration["Steam:GroupId"]!;
    }

    public async Task<bool> IsUserInGroupAsync(string steamId)
    {
        try
        {
            return await FallbackXmlAsync(steamId);

            // var url = $"https://partner.steam-ap/* i.com/ISteamUser/GetUserGroupList/v1/?key={_apiKey}&steamid={steamId}";
            // var response = await _httpClient.GetAsync(url);

            // if (!response.IsSuccessStatusCode) */
            //     return await FallbackXmlAsync(steamId);

            // var json = await response.Content.ReadAsStringAsync();
            // var doc = JsonDocument.Parse(json);
            // var groups = doc.RootElement
            //     .GetProperty("response")
            //     .GetProperty("groups")
            //     .EnumerateArray();

            // return groups.Any(g => g.GetProperty("gid").GetString() == _groupId);
        }
        catch
        {
            return await FallbackXmlAsync(steamId);
        }
    }

    private async Task<bool> FallbackXmlAsync(string steamId)
    {
        try
        {
            var url = $"https://steamcommunity.com/gid/{_groupId}/memberslistxml/?xml=1";
            var response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode) return false;

            var xml = await response.Content.ReadAsStringAsync();
            return xml.Contains(steamId);
        }
        catch
        {
            return false;
        }
    }

    public async Task<string?> ResolveSteamIdAsync(string profileUrl)
    {
        try
        {
            if (profileUrl.Contains("/profiles/")) //ID numérico (/profiles/76561198XXXXXXXXX)
            {
                return profileUrl.TrimEnd('/').Split('/').Last();
            }

            if (profileUrl.Contains("/id/")) //ID personalizado (/id/nomePersonalizado)
            {
                var vanityId = profileUrl.TrimEnd('/').Split('/').Last();
                var url = $"https://api.steampowered.com/ISteamUser/ResolveVanityURL/v1/?key={_apiKey}&vanityurl={vanityId}";
                var response = await _httpClient.GetAsync(url);
                if (!response.IsSuccessStatusCode) return null;

                var json = await response.Content.ReadAsStringAsync();
                var doc = JsonDocument.Parse(json);
                var success = doc.RootElement.GetProperty("response").GetProperty("success").GetInt32();
                if (success != 1) return null;

                return doc.RootElement.GetProperty("response").GetProperty("steamid").GetString();
            }

            return null;
        }
        catch
        {
            return null;
        }
    }
}
