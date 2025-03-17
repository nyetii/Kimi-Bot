using System.ComponentModel.DataAnnotations;

namespace Kimi.Repository.Models;

public class Guild
{
    [Key]
    public ulong Id { get; init; }

    [MaxLength(256)]
    public string Name { get; init; } = "unknown";

    public ICollection<User> Users { get; init; } = [];
    public ICollection<GuildUser> GuildUsers { get; init; } = [];
    public ICollection<DailyScore> DailyScores { get; init; } = [];
}