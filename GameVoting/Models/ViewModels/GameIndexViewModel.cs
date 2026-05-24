namespace GameVoting.Models.ViewModels;

public class GameIndexViewModel
{
    public IEnumerable<RankingItemViewModel> Ranking { get; set; } = new List<RankingItemViewModel>();
}
