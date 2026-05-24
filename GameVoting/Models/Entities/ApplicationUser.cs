using Microsoft.AspNetCore.Identity;

namespace GameVoting.Models.Entities;

public class ApplicationUser : IdentityUser
{
    public string DisplayName { get; set; } = string.Empty;
    public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;
    public string SteamId { get; set; } = string.Empty;
    public string? AdminAlias { get; set; }
}
