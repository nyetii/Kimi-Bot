using System.ComponentModel.DataAnnotations;

namespace Kimi.Repository.Models;

public class Guild
{
    [Key]
    public ulong Id { get; set; }

    public string Name { get; set; } = "unknown";

    public ICollection<User> Users { get; set; } = [];
    public ICollection<GuildUser> GuildUsers { get; set; } = [];
    public ICollection<DailyScore> DailyScores { get; set; } = [];
}