using System.ComponentModel.DataAnnotations;
using Discord.WebSocket;

namespace Kimi.Repository.Models;

public class User
{
    [Key] public ulong Id { get; init; }

    [MaxLength(32)] public string Username { get; set; } = "unknown";
    public DateOnly? Birthday { get; set; }
    public ICollection<Guild> Guilds { get; init; } = [];
    public ICollection<GuildUser> GuildUsers { get; init; } = [];
    public ICollection<DailyScore> DailyScores { get; init; } = [];

    public GuildUser GetGuildUserInfo(ulong guildId)
        => GuildUsers.FirstOrDefault(x => x.GuildId == guildId) 
           ?? throw new Exception($"Could not get info for {Username} [{Id}] on Guild with ID {guildId}");

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