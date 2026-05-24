using GameVoting.Data;
using GameVoting.Models.Entities;
using GameVoting.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GameVoting.Repositories;

public class GameRepository : IGameRepository
{
    private readonly AppDbContext _context;

    public GameRepository(AppDbContext context)
    {
        _context = context;
    }

    public IEnumerable<Game> GetAll()
    {
        return _context.Games
            .Include(g => g.Votes)
            .ToList();
    }

    public Game? GetById(int id)
    {
        return _context.Games.Find(id);
    }
    public void Add(Game game)
    {
        _context.Add(game);
        _context.SaveChanges();
    }

    public void Update(Game game)
    {
        _context.Update(game);
        _context.SaveChanges();
    }

    public void Remove(Game game)
    {
        _context.Remove(game);
        _context.SaveChanges();
    }
}
