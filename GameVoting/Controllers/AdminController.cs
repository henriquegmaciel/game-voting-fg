using GameVoting.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GameVoting.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly IAdminService _adminService;
    private readonly IRegistrationTokenService _tokenService;

    public AdminController(IAdminService adminService, IRegistrationTokenService tokenService)
    {
        _adminService = adminService;
        _tokenService = tokenService;
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
    public IActionResult GenerateToken(string label)
    {
        if (string.IsNullOrWhiteSpace(label))
        {
            TempData["Erro"] = "Informe uma identificação para o token.";
            return RedirectToAction("Index");
        }

        _tokenService.GenerateToken(label);
        TempData["Sucesso"] = "Token gerado com sucesso.";
        return RedirectToAction("Index");
    }

    [HttpPost]
    public IActionResult RevokeToken(int tokenId)
    {
        _tokenService.RevokeToken(tokenId);
        TempData["Sucesso"] = "Token revogado.";
        return RedirectToAction("Index");
    }
}
