using GameVoting.Data;
using GameVoting.Models.Entities;
using GameVoting.Repositories.Interfaces;

namespace GameVoting.Repositories;

public class RegistrationTokenRepository : IRegistrationTokenRepository
{
    private readonly AppDbContext _context;

    public RegistrationTokenRepository(AppDbContext context) => _context = context;

    public RegistrationToken? GetByToken(string token)
        => _context.RegistrationTokens.FirstOrDefault(t => t.Token == token);

    public IEnumerable<RegistrationToken> GetAll()
        => _context.RegistrationTokens.OrderByDescending(t => t.CreatedAt).ToList();

    public void Add(RegistrationToken token)
    {
        _context.RegistrationTokens.Add(token);
        _context.SaveChanges();
    }

    public void Update(RegistrationToken token)
    {
        _context.RegistrationTokens.Update(token);
        _context.SaveChanges();
    }

    public RegistrationToken? GetActiveBySteamId(string steamId)
    => _context.RegistrationTokens
        .FirstOrDefault(t => t.SteamId == steamId && !t.IsUsed);
}
