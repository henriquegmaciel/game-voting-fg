namespace GameVoting.Services.Interfaces;

public interface ISteamService
{
    Task<bool> IsUserInGroupAsync(string steamId);
    Task<string?> ResolveSteamIdAsync(string profileUrl);
}
