using GameVoting.Models.Entities;
using GameVoting.Repositories.Interfaces;
using GameVoting.Services.Interfaces;

namespace GameVoting.Services;

public class RegistrationTokenService : IRegistrationTokenService
{
    private readonly IRegistrationTokenRepository _repository;

    public RegistrationTokenService(IRegistrationTokenRepository repository)
    {
        _repository = repository;
    }

    public void GenerateToken(string label)
    {
        _repository.Add(new RegistrationToken
        {
            Token = Guid.NewGuid().ToString(),
            Label = label,
            CreatedAt = DateTime.UtcNow
        });
    }

    public IEnumerable<RegistrationToken> GetAll()
        => _repository.GetAll();

    public void RevokeToken(int tokenId)
    {
        var token = _repository.GetAll().FirstOrDefault(t => t.Id == tokenId);
        if (token is null) return;
        token.IsUsed = true;
        token.UsedAt = DateTime.UtcNow;
        _repository.Update(token);
    }

    public bool ValidateToken(string token)
    {
        var t = _repository.GetByToken(token);
        return t is not null && !t.IsUsed;
    }

    public void MarkAsUsed(string token)
    {
        var t = _repository.GetByToken(token);
        if (t is null) return;
        t.IsUsed = true;
        t.UsedAt = DateTime.UtcNow;
        _repository.Update(t);
    }
}
