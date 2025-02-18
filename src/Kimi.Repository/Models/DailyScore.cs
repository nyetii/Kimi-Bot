using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Kimi.Repository.Models;

[Index(nameof(UserId), nameof(GuildId), nameof(Date), IsUnique = true)]
public class DailyScore
{
    [Key]
    public ulong Id { get; init; }

    public ulong UserId { get; init; }
    public User User { get; init; } = null!;

    public ulong GuildId { get; init; }
    public Guild Guild { get; init; } = null!;

    public DateOnly Date { get; init; }
    public uint Score { get; set; }
    public uint MessageCount { get; set; }
}