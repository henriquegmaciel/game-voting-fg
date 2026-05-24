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
    private readonly IUserService _userService;

    public AuthController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        ISteamService steamService,
        IRegistrationTokenService tokenService,
        IUserService userService)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _steamService = steamService;
        _tokenService = tokenService;
        _userService = userService;
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

        if (!string.IsNullOrEmpty(token.SteamId))
        {
            var existingUser = await _userService.FindBySteamIdAsync(token.SteamId);
            if (existingUser is not null)
            {
                ModelState.AddModelError(string.Empty, "Este token está vinculado a um perfil Steam já cadastrado.");
                return View(model);
            }
        }

        var user = new ApplicationUser
        {
            UserName = model.Username,
            DisplayName = model.Username,
            AdminAlias = token.Label,
            SteamId = token.SteamId!,
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

        if (!await _steamService.IsUserInGroupAsync(steamId))
            return await DenySteamAccess();

        var result = await _signInManager.ExternalLoginSignInAsync(
            info.LoginProvider, info.ProviderKey, isPersistent: true);

        if (result.Succeeded)
            return await HandleExistingSteamLogin(steamId, steamName, returnUrl);

        var existingBySteamId = await _userService.FindBySteamIdAsync(steamId);
        if (existingBySteamId is not null)
            return await HandleTokenUserSteamLink(existingBySteamId, info, steamId, steamName, returnUrl);

        return await HandleNewSteamUser(info, steamId, steamName, returnUrl);
    }

    private async Task<IActionResult> DenySteamAccess()
    {
        await _signInManager.SignOutAsync();
        TempData["Erro"] = "Você precisa ser membro do grupo Steam para acessar.";
        return RedirectToAction("Login");
    }

    private async Task<IActionResult> HandleExistingSteamLogin(string steamId, string steamName, string? returnUrl)
    {
        _tokenService.RevokeBySteamId(steamId);
        var user = await _userManager.FindByNameAsync(steamId);
        if (user is not null && user.DisplayName != steamName)
        {
            await UpdateUserDisplayNameAsync(user, steamName);
            await UpdateDisplayNameClaimAsync(user, steamName);
        }
        return LocalRedirect(returnUrl ?? "/");
    }

    private async Task<IActionResult> HandleTokenUserSteamLink(
        ApplicationUser user,
        ExternalLoginInfo info,
        string steamId,
        string steamName,
        string? returnUrl)
    {
        await _userManager.AddLoginAsync(user, info);
        await UpdateUserDisplayNameAsync(user, steamName);
        await EnsureDisplayNameClaimAsync(user, steamName);
        await _signInManager.SignInAsync(user, isPersistent: true);
        _tokenService.RevokeBySteamId(steamId);
        return LocalRedirect(returnUrl ?? "/");
    }

    private async Task<IActionResult> HandleNewSteamUser(
        ExternalLoginInfo info,
        string steamId,
        string steamName,
        string? returnUrl)
    {
        var user = new ApplicationUser
        {
            UserName = steamId,
            DisplayName = steamName,
            SteamId = steamId,
            RegisteredAt = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(user);
        if (!result.Succeeded)
            return RedirectToAction("Login");

        await _userManager.AddLoginAsync(user, info);
        await _userManager.AddClaimAsync(user, new Claim("DisplayName", steamName));
        await _signInManager.SignInAsync(user, isPersistent: true);
        _tokenService.RevokeBySteamId(steamId);
        return LocalRedirect(returnUrl ?? "/");
    }

    private async Task UpdateUserDisplayNameAsync(ApplicationUser user, string steamName)
    {
        user.DisplayName = steamName;
        await _userManager.UpdateAsync(user);
    }

    private async Task UpdateDisplayNameClaimAsync(ApplicationUser user, string steamName)
    {
        var claims = await _userManager.GetClaimsAsync(user);
        var existing = claims.FirstOrDefault(c => c.Type == "DisplayName");
        if (existing is not null)
            await _userManager.ReplaceClaimAsync(user, existing, new Claim("DisplayName", steamName));
    }

    private async Task EnsureDisplayNameClaimAsync(ApplicationUser user, string steamName)
    {
        var claims = await _userManager.GetClaimsAsync(user);
        if (!claims.Any(c => c.Type == "DisplayName"))
            await _userManager.AddClaimAsync(user, new Claim("DisplayName", steamName));
    }
}
