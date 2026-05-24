using GameVoting.Models.Entities;

namespace GameVoting.Repositories.Interfaces;

public interface IRegistrationTokenRepository
{
    RegistrationToken? GetByToken(string token);
    IEnumerable<RegistrationToken> GetAll();
    void Add(RegistrationToken token);
    void Update(RegistrationToken token);
    RegistrationToken? GetActiveBySteamId(string steamId);
}
