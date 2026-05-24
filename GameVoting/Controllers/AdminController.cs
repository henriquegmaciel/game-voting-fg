using GameVoting.Models.Entities;
using GameVoting.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace GameVoting.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly IAdminService _adminService;
    private readonly IRegistrationTokenService _tokenService;
    private readonly UserManager<ApplicationUser> _userManager;

    public AdminController(
        IAdminService adminService,
        IRegistrationTokenService tokenService,
        UserManager<ApplicationUser> userManager)
    {
        _adminService = adminService;
        _tokenService = tokenService;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var viewModel = await _adminService.GetAdminViewModel();
        viewModel.Tokens = _tokenService.GetAll();
        return View(viewModel);
    }

    [HttpPost]
    public async Task<IActionResult> RemoveMember(string userId)
    {
        await _adminService.RemoveMember(userId);
        TempData["Sucesso"] = "Membro removido.";
        return RedirectToAction("Index");
    }

    [HttpPost]
    public async Task<IActionResult> RemoveVote(int voteId)
    {
        await _adminService.RemoveVote(voteId);
        TempData["Sucesso"] = "Voto removido.";
        return RedirectToAction("Index");
    }

    [HttpPost]
    public async Task<IActionResult> GenerateToken(string label, string steamProfileUrl)
    {
        if (string.IsNullOrWhiteSpace(label))
        {
            TempData["Erro"] = "Informe um apelido para o usuário.";
            return RedirectToAction("Index");
        }
        if (string.IsNullOrWhiteSpace(steamProfileUrl))
        {
            TempData["Erro"] = "Informe o link do perfil do membro.";
            return RedirectToAction("Index");
        }

        var (success, message) = await _tokenService.GenerateTokenAsync(label, steamProfileUrl);
        TempData[success ? "Sucesso" : "Erro"] = message;
        return RedirectToAction("Index");
    }

    [HttpPost]
    public IActionResult RevokeToken(int tokenId)
    {
        _tokenService.RevokeToken(tokenId);
        TempData["Sucesso"] = "Token revogado.";
        return RedirectToAction("Index");
    }

    [HttpPost]
    public async Task<IActionResult> UpdateAlias(string userId, string? alias)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null) return NotFound();

        user.AdminAlias = alias;
        await _userManager.UpdateAsync(user);
        TempData["Sucesso"] = "Apelido atualizado.";
        return RedirectToAction("Index");
    }
}
