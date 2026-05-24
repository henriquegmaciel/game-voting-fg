using GameVoting.Models.Entities;

namespace GameVoting.Services.Interfaces;

public interface IUserService
{
    Task<ApplicationUser?> FindBySteamIdAsync(string steamId);
}
