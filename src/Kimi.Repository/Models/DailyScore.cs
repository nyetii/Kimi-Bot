using System.ComponentModel.DataAnnotations;

namespace Kimi.Repository.Models;

public class DailyScore
{
    [Key]
    public ulong Id { get; set; }

    public ulong UserId { get; set; }
    public User User { get; set; } = null!;

    public ulong GuildId { get; set; }
    public Guild Guild { get; set; } = null!;

    public DateOnly Date { get; set; }
    public uint Score { get; set; }
}