namespace GameVoting.Services.Interfaces;

public interface ISteamService
{
    Task<bool> IsUserInGroupAsync(string steamId);
}
