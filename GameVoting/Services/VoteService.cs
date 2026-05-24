using GameVoting.Models.Entities;
using GameVoting.Repositories.Interfaces;
using GameVoting.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace GameVoting.Services;

[Authorize]
public class VoteService : IVoteService
{
    private readonly IVoteRepository _repository;

    public VoteService(IVoteRepository repository)
    {
        _repository = repository;
    }

    public bool Vote(string userId, int gameId)
    {
        if (_repository.HasVoted(userId, gameId))
            return false;

        _repository.Add(new Vote
        {
            GameId = gameId,
            UserId = userId,
            VotedAt = DateTime.UtcNow
        });

        return true;
    }

    public void RemoveVote(string userId, int gameId)
    {
        Vote? vote = _repository.GetByUserAndGame(userId, gameId);
        if (vote is null) return;
        _repository.Remove(vote);
    }
}
