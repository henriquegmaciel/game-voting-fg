using GameVoting.Models.Entities;
using GameVoting.Repositories.Interfaces;
using GameVoting.Services.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace GameVoting.Services;

public class RegistrationTokenService : IRegistrationTokenService
{
    private readonly IRegistrationTokenRepository _repository;
    private readonly ISteamService _steamService;
    private readonly IUserService _userService;

    public RegistrationTokenService(
        IRegistrationTokenRepository repository,
        ISteamService steamService,
        IUserService userService)
    {
        _repository = repository;
        _steamService = steamService;
        _userService = userService;
    }

    public async Task<(bool Success, string Message)> GenerateTokenAsync(string label, string? steamProfileUrl)
    {
        string? steamId = null;

        if (!string.IsNullOrWhiteSpace(steamProfileUrl))
        {
            steamId = await _steamService.ResolveSteamIdAsync(steamProfileUrl);

            if (steamId is not null)
            {
                var existingUser = await _userService.FindBySteamIdAsync(steamId);
                if (existingUser is not null)
                    return (false, "Já existe um usuário cadastrado com esse perfil Steam.");
            }
        }

        _repository.Add(new RegistrationToken
        {
            Token = Guid.NewGuid().ToString(),
            Label = label,
            SteamId = steamId,
            CreatedAt = DateTime.UtcNow
        });

        return (true, "Token gerado com sucesso.");
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

    public void RevokeBySteamId(string steamId)
    {
        var token = _repository.GetActiveBySteamId(steamId);
        if (token is null) return;
        token.IsUsed = true;
        token.UsedAt = DateTime.UtcNow;
        _repository.Update(token);
    }
}
