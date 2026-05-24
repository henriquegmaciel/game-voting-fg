using GameVoting.Models.Entities;

namespace GameVoting.Services.Interfaces;

public interface IRegistrationTokenService
{
    void GenerateToken(string label);
    IEnumerable<RegistrationToken> GetAll();
    void RevokeToken(int tokenId);
    bool ValidateToken(string token);
    void MarkAsUsed(string token);
}
