using GameVoting.Data;
using GameVoting.Models.Entities;
using GameVoting.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GameVoting.Repositories;

public class VoteRepository : IVoteRepository
{
    private readonly AppDbContext _context;

    public VoteRepository(AppDbContext context)
    {
        _context = context;
    }
    public bool HasVoted(string userId, int gameId)
    {
        return _context.Votes
            .Any(v =>
                v.UserId == userId
                && v.GameId == gameId);
    }

    public Vote? GetByUserAndGame(string userId, int gameId)
    {
        return _context.Votes
            .FirstOrDefault(v =>
                v.UserId == userId
                && v.GameId == gameId);
    }

    public void Add(Vote vote)
    {
        _context.Add(vote);
        _context.SaveChanges();
    }

    public void Remove(Vote vote)
    {
        _context.Remove(vote);
        _context.SaveChanges();
    }

    public void RemoveAllByGame(int gameId)
    {
        IEnumerable<Vote> votes = _context.Votes
            .Where(v => v.GameId == gameId)
            .ToList();

        _context.Votes.RemoveRange(votes);
        _context.SaveChanges();
    }

    public HashSet<int> GetVotedGameIdsByUser(string userId)
    {
        return _context.Votes
            .Where(v => v.UserId == userId)
            .Select(v => v.GameId)
            .ToHashSet();
    }

    public IEnumerable<Vote> GetAllWithUsers()
    {
        return _context.Votes
            .Include(v => v.User)
            .AsNoTracking()
            .ToList();
    }

    public Vote? GetById(int id)
    {
        return _context.Votes.Find(id);
    }

    public void RemoveAllByUser(string userId)
    {
        var votes = _context.Votes.Where(v => v.UserId == userId).ToList();
        _context.Votes.RemoveRange(votes);
        _context.SaveChanges();
    }
}
