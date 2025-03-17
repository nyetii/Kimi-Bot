using Discord;
using Discord.WebSocket;

namespace Kimi.Repository.Dtos;

public record GuildDto
{
    public ulong Id { get; init; }
    public string Name { get; init; }

    public GuildDto(IGuild guild)
    {
        Id = guild.Id;
        Name = guild.Name;
    }
}