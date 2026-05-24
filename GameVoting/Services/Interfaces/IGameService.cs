using GameVoting.Models.Entities;
using GameVoting.Models.ViewModels;

namespace GameVoting.Services.Interfaces;

public interface IGameService
{
    GameIndexViewModel GetRanking(string? userId, ListType? listType = null);
    Game? GetById(int id);
    void AddGame(Game game);
    void RemoveGame(int gameId);
    void UpdateGame(EditGameViewModel model);
}
