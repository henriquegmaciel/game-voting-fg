using GameVoting.Models.Entities;

namespace GameVoting.Models.ViewModels;

public class RankingItemViewModel
{
    public int GameId { get; set; }
    public string Title { get; set; } = string.Empty;
    public ListType ListType { get; set; }
    public string? Genre { get; set; }
    public int? ReleaseYear { get; set; }
    public string? ImageUrl { get; set; }
    public int VoteCount { get; set; }
    public bool UserAlreadyVoted { get; set; }
    public string? StorePageUrl { get; set; }
}
