using GameVoting.Models.Entities;
using GameVoting.Models.ViewModels;
using GameVoting.Repositories.Interfaces;
using GameVoting.Services.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace GameVoting.Services;

public class AdminService : IAdminService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IVoteRepository _voteRepository;
    private readonly IGameRepository _gameRepository;

    public AdminService(
        UserManager<ApplicationUser> userManager,
        IVoteRepository voteRepository,
        IGameRepository gameRepository)
    {
        _userManager = userManager;
        _voteRepository = voteRepository;
        _gameRepository = gameRepository;
    }

    public async Task<AdminIndexViewModel> GetAdminViewModel()
    {
        var users = _userManager.Users.ToList();
        var members = new List<AdminMemberViewModel>();

        foreach (var user in users)
        {
            var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
            members.Add(new AdminMemberViewModel
            {
                UserId = user.Id,
                DisplayName = user.DisplayName,
                RegisteredAt = user.RegisteredAt,
                IsAdmin = isAdmin
            });
        }

        var votes = _voteRepository.GetAllWithUsers().ToList();
        var games = _gameRepository.GetAll().ToList();

        var gameVotes = games.Select(g => new AdminGameVotesViewModel
        {
            GameId = g.Id,
            Title = g.Title,
            ListType = g.ListType,
            Votes = votes
                .Where(v => v.GameId == g.Id)
                .Select(v => new AdminVoteViewModel
                {
                    VoteId = v.Id,
                    UserId = v.UserId!,
                    DisplayName = v.User?.DisplayName ?? v.UserId!,
                    VotedAt = v.VotedAt
                })
        });

        return new AdminIndexViewModel
        {
            Members = members,
            GameVotes = gameVotes
        };
    }

    public async Task RemoveVote(int voteId)
    {
        var vote = _voteRepository.GetById(voteId);
        if (vote is null) return;
        _voteRepository.Remove(vote);
    }

    public async Task RemoveMember(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null) return;

        _voteRepository.RemoveAllByUser(userId);
        await _userManager.DeleteAsync(user);
    }
}
