using GameVoting.Models.ViewModels;

namespace GameVoting.Services.Interfaces;

public interface IAdminService
{
    Task<AdminIndexViewModel> GetAdminViewModel();
    Task RemoveVote(int voteId);
    Task RemoveMember(string userId);
}
