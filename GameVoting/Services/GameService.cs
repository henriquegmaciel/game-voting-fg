using GameVoting.Models.Entities;
using GameVoting.Models.ViewModels;
using GameVoting.Repositories.Interfaces;
using GameVoting.Services.Interfaces;

namespace GameVoting.Services;

public class GameService : IGameService
{
    private readonly IGameRepository _gameRepository;
    private readonly IVoteRepository _voteRepository;

    public GameService(IGameRepository gameRepository, IVoteRepository voteRepository)
    {
        _gameRepository = gameRepository;
        _voteRepository = voteRepository;
    }

    public GameIndexViewModel GetRanking(string? userId, ListType? listType = null)
    {
        var games = _gameRepository.GetAll();

        if (listType.HasValue)
            games = games.Where(g => g.ListType == listType.Value);

        HashSet<int> userVotedGameIds = userId is not null
            ? _voteRepository.GetVotedGameIdsByUser(userId)
            : new HashSet<int>();

        var toRankingItem = (Game g) => new RankingItemViewModel
        {
            GameId = g.Id,
            Title = g.Title,
            ListType = g.ListType,
            Genre = g.Genre,
            ReleaseYear = g.ReleaseYear,
            ImageUrl = g.ImageUrl,
            StorePageUrl = g.StorePageUrl,
            VoteCount = g.Votes.Count,
            UserAlreadyVoted = userVotedGameIds.Contains(g.Id)
        };

        return new GameIndexViewModel
        {
            Ranking = games
                .OrderByDescending(g => g.Votes.Count)
                .Select(toRankingItem)
        };
    }

    public Game? GetById(int id)
    {
        return _gameRepository.GetById(id);
    }

    public void AddGame(Game game)
    {
        _gameRepository.Add(game);
    }

    public void RemoveGame(int gameId)
    {
        Game? game = _gameRepository.GetById(gameId);
        if (game is not null)
            _gameRepository.Remove(game);
    }

    public void UpdateGame(EditGameViewModel model)
    {
        var game = _gameRepository.GetById(model.Id);
        if (game is null) return;

        game.Title = model.Title;
        game.Genre = model.Genre;
        game.ReleaseYear = model.ReleaseYear;
        game.ImageUrl = model.ImageUrl;
        game.StorePageUrl = model.StorePageUrl;
        game.ListType = model.ListType;

        _gameRepository.Update(game);
    }
}
