using System.ComponentModel.DataAnnotations;
using Discord.WebSocket;

namespace Kimi.Repository.Models;

public class User
{
    [Key]
    public ulong Id { get; init; }
    
    [MaxLength(32)]
    public string Username { get; set; } = "unknown";
    public DateOnly? Birthday { get; set; }
    public ICollection<Guild> Guilds { get; init; } = [];
    public ICollection<GuildUser> GuildUsers { get; init; } = [];
    public ICollection<DailyScore> DailyScores { get; init; } = [];

    public GuildUser GetGuildUserInfo(ulong guildId)
        => GuildUsers.First(x => x.GuildId == guildId);
    
    public static User ToEntity(SocketUserMessage message)
    {
        return new User
        {
            Id = message.Author.Id,
            Username = message.Author.Username,
            Guilds = [],
            DailyScores = [],
        };
    }
}