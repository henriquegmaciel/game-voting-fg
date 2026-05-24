using GameVoting.Models.Entities;
using GameVoting.Services.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace GameVoting.Services;

public class UserService : IUserService
{
    private readonly UserManager<ApplicationUser> _userManager;

    public UserService(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public Task<ApplicationUser?> FindBySteamIdAsync(string steamId)
    {
        return Task.FromResult(
            _userManager.Users.FirstOrDefault(u => u.SteamId == steamId));
    }
}
