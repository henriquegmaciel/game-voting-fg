using System.Security.Claims;
using GameVoting.Models.Entities;
using GameVoting.Models.ViewModels;
using GameVoting.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace GameVoting.Controllers;

public class AuthController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly ISteamService _steamService;
    private readonly IRegistrationTokenService _tokenService;

    public AuthController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        ISteamService steamService,
        IRegistrationTokenService tokenService)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _steamService = steamService;
        _tokenService = tokenService;
    }

    [HttpGet]
    public IActionResult Register() => View();

    [HttpPost]
    public async Task<IActionResult> Register(RegisterWithTokenViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        if (!_tokenService.ValidateToken(model.Token))
        {
            ModelState.AddModelError("Token", "Token inválido ou já utilizado.");
            return View(model);
        }

        var token = _tokenService.GetAll().First(t => t.Token == model.Token);

        var user = new ApplicationUser
        {
            UserName = model.Username,
            DisplayName = token.Label,
            RegisteredAt = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(user, model.Password);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);
            return View(model);
        }

        _tokenService.MarkAsUsed(model.Token);
        await _signInManager.SignInAsync(user, isPersistent: true);
        return RedirectToAction("Index", "Game");
    }

    [HttpGet]
    public IActionResult Login() => View();

    [HttpPost]
    public async Task<IActionResult> Login(LoginWithPasswordViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var result = await _signInManager.PasswordSignInAsync(
            model.Username, model.Password, isPersistent: true, lockoutOnFailure: false);

        if (result.Succeeded)
        {
            var user = await _userManager.FindByNameAsync(model.Username);
            var claims = await _userManager.GetClaimsAsync(user!);
            if (!claims.Any(c => c.Type == "DisplayName"))
                await _userManager.AddClaimAsync(user!, new Claim("DisplayName", user!.DisplayName));

            await _signInManager.RefreshSignInAsync(user!);
            return RedirectToAction("Index", "Game");
        }

        ModelState.AddModelError(string.Empty, "Usuário ou senha inválidos.");
        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Index", "Game");
    }

    [HttpGet]
    public IActionResult SteamLogin(string? returnUrl = null)
    {
        var redirectUrl = Url.Action("SteamCallback", "Auth", new { returnUrl });
        var properties = _signInManager.ConfigureExternalAuthenticationProperties("Steam", redirectUrl);
        return Challenge(properties, "Steam");
    }

    [HttpGet]
    public async Task<IActionResult> SteamCallback(string? returnUrl = null)
    {
        var info = await _signInManager.GetExternalLoginInfoAsync();
        if (info is null)
            return RedirectToAction("Login");

        var steamId = info.ProviderKey.Split('/').Last();
        var steamName = info.Principal.Identity?.Name ?? steamId;

        var isInGroup = await _steamService.IsUserInGroupAsync(steamId);
        if (!isInGroup)
        {
            await _signInManager.SignOutAsync();
            TempData["AuthErro"] = "Você precisa ser membro do grupo Steam para acessar.";
            return RedirectToAction("Login");
        }

        var result = await _signInManager.ExternalLoginSignInAsync(
            info.LoginProvider, info.ProviderKey, isPersistent: true);

        if (result.Succeeded)
        {
            var existingUser = await _userManager.FindByNameAsync(steamId);
            if (existingUser is not null && existingUser.DisplayName != steamName)
            {
                UpdateUserDisplayName(existingUser, steamName);
                UpdateDisplayNameClaim(existingUser, steamName);
            }
            return LocalRedirect(returnUrl ?? "/");
        }

        var user = new ApplicationUser
        {
            UserName = steamId,
            DisplayName = steamName,
            RegisteredAt = DateTime.UtcNow
        };

        var createResult = await _userManager.CreateAsync(user);
        if (createResult.Succeeded)
        {
            await _userManager.AddLoginAsync(user, info);
            await _userManager.AddClaimAsync(user, new Claim("DisplayName", steamName));
            await _signInManager.SignInAsync(user, isPersistent: true);

            return LocalRedirect(returnUrl ?? "/");
        }

        return RedirectToAction("Login");
    }

    private async void UpdateUserDisplayName(ApplicationUser user, string newName)
    {
        user.DisplayName = newName;
        await _userManager.UpdateAsync(user);
    }

    private async void UpdateDisplayNameClaim(ApplicationUser user, string newName)
    {
        var oldClaim = await GetDisplayNameClaim(user);
        if (oldClaim is not null)
            await _userManager.ReplaceClaimAsync(user, oldClaim, new Claim("DisplayName", newName));
        else
            await _userManager.AddClaimAsync(user, new Claim("DisplayName", newName));
    }

    private async Task<Claim?> GetDisplayNameClaim(ApplicationUser user)
    {
        return (await _userManager.GetClaimsAsync(user))
                    .FirstOrDefault(c => c.Type == "DisplayName");
    }
}
