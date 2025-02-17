using System.ComponentModel.DataAnnotations;
using Discord.WebSocket;

namespace Kimi.Repository.Models;

public class User
{
    [Key]
    public ulong Id { get; set; }
    public string Username { get; set; } = "unknown";
    public ICollection<Guild> Guilds { get; set; } = [];
    public ICollection<GuildUser> GuildsUsers { get; set; } = [];
    public ICollection<DailyScore> DailyScores { get; set; } = [];

    public GuildUser GetGuildUserInfo(ulong guildId)
        => GuildsUsers.First(x => x.GuildId == guildId);
    
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