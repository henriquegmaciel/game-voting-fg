using GameVoting.Models.Entities;

namespace GameVoting.Repositories.Interfaces;

public interface IGameRepository
{
    IEnumerable<Game> GetAll();
    Game? GetById(int id);
    void Add(Game game);
    void Update(Game game);
    void Remove(Game game);
}
