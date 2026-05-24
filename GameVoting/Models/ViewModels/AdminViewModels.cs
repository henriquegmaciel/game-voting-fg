using GameVoting.Models.Entities;

namespace GameVoting.Models.ViewModels;

public class AdminMemberViewModel
{
    public string UserId { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public DateTime RegisteredAt { get; set; }
    public bool IsAdmin { get; set; }
}

public class AdminIndexViewModel
{
    public IEnumerable<AdminMemberViewModel> Members { get; set; } = new List<AdminMemberViewModel>();
    public IEnumerable<AdminGameVotesViewModel> GameVotes { get; set; } = new List<AdminGameVotesViewModel>();
    public IEnumerable<RegistrationToken> Tokens { get; set; } = new List<RegistrationToken>();
}

public class AdminGameVotesViewModel
{
    public int GameId { get; set; }
    public string Title { get; set; } = string.Empty;
    public ListType ListType { get; set; }
    public IEnumerable<AdminVoteViewModel> Votes { get; set; } = new List<AdminVoteViewModel>();
}

public class AdminVoteViewModel
{
    public int VoteId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public DateTime VotedAt { get; set; }
}
