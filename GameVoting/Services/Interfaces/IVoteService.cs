namespace GameVoting.Services.Interfaces;

public interface IVoteService
{
    bool Vote(string userId, int gameId);
    void RemoveVote(string userId, int gameId);
}
