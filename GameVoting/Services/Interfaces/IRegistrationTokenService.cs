using GameVoting.Models.Entities;

namespace GameVoting.Services.Interfaces;

public interface IRegistrationTokenService
{
    Task<(bool Success, string Message)> GenerateTokenAsync(string label, string? steamProfileUrl);
    IEnumerable<RegistrationToken> GetAll();
    void RevokeToken(int tokenId);
    bool ValidateToken(string token);
    void MarkAsUsed(string token);
    void RevokeBySteamId(string steamId);
}
