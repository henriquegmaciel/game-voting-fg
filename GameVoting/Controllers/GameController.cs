using System.Security.Claims;
using GameVoting.Models.Entities;
using GameVoting.Models.ViewModels;
using GameVoting.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GameVoting.Controllers;

[Authorize]
public class GameController : Controller
{
    private readonly IGameService _gameService;
    public GameController(IGameService gameService)
    {
        _gameService = gameService;
    }

    public IActionResult Index() => RedirectToAction("Coop");

    public IActionResult Coop()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var viewModel = _gameService.GetRanking(userId, ListType.Coop);
        ViewData["ActiveList"] = "Coop";
        return View("Index", viewModel);
    }

    public IActionResult Multiplayer()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var viewModel = _gameService.GetRanking(userId, ListType.Multiplayer);
        ViewData["ActiveList"] = "Multiplayer";
        return View("Index", viewModel);
    }

    [Authorize(Roles = "Admin")]
    public IActionResult All()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var viewModel = _gameService.GetRanking(userId, null);
        ViewData["ActiveList"] = "All";
        return View("Index", viewModel);
    }

    [Authorize(Roles = "Admin")]
    public IActionResult Create()
    {
        return View();
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public IActionResult Create(CreateGameViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        _gameService.AddGame(new Game
        {
            Title = model.Title,
            ListType = model.ListType!.Value,
            Genre = model.Genre,
            ReleaseYear = model.ReleaseYear,
            ImageUrl = model.ImageUrl,
            StorePageUrl = model.StorePageUrl,
            AddedAt = DateTime.UtcNow
        });

        return RedirectToAction("Index");
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public IActionResult Remove(int gameId)
    {
        _gameService.RemoveGame(gameId);
        return RedirectToAction("Index", "Game");
    }

    [HttpGet]
    public IActionResult Edit(int id)
    {
        var game = _gameService.GetById(id);
        if (game is null) return NotFound();

        return View(new EditGameViewModel
        {
            Id = game.Id,
            Title = game.Title,
            Genre = game.Genre,
            ReleaseYear = game.ReleaseYear,
            ImageUrl = game.ImageUrl,
            StorePageUrl = game.StorePageUrl,
            ListType = game.ListType
        });
    }

    [HttpPost]
    public IActionResult Edit(EditGameViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        _gameService.UpdateGame(model);
        TempData["Sucesso"] = "Jogo atualizado!";
        return RedirectToAction("Index");
    }
}
