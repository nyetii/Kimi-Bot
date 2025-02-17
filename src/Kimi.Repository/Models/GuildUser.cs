namespace Kimi.Repository.Models;

public class GuildUser
{
    public ulong GuildId { get; set; }
    public Guild Guild { get; set; } = null!;

    public ulong UserId { get; set; }
    public User User { get; set; } = null!;

    public string? Nickname { get; set; }
    public DateTime? LastMessage { get; set; }
}