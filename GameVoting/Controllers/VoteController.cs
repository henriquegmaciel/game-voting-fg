using GameVoting.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace GameVoting.Controllers;

[Authorize]
public class VoteController : Controller
{
    private readonly IVoteService _voteService;

    public VoteController(IVoteService voteService)
    {
        _voteService = voteService;
    }

    [HttpPost]
    public async Task<IActionResult> Cast(int gameId)
    {
        string userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        bool voted = _voteService.Vote(userId, gameId);

        return Json(
            new
            {
                success = voted,
                message = voted ? "" : "Você já votou neste jogo."
            });
    }

    [HttpPost]
    public async Task<IActionResult> Remove(int gameId)
    {
        string userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        _voteService.RemoveVote(userId, gameId);

        return Json(new { success = true });
    }
}
