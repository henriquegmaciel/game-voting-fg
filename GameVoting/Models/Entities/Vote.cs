using System.ComponentModel.DataAnnotations.Schema;

namespace GameVoting.Models.Entities;

public class Vote
{
    public int Id { get; set; }
    public int GameId { get; set; }
    public DateTime VotedAt { get; set; } = DateTime.UtcNow;
    [ForeignKey("User")]
    public string? UserId { get; set; }
    public ApplicationUser? User { get; set; }
}
