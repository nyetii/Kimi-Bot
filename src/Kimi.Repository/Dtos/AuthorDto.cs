using Discord;
using Discord.WebSocket;
using Kimi.Repository.Models;

namespace Kimi.Repository.Dtos;

public record AuthorDto
{
    public ulong Id { get; init; }
    public string Username { get; init; }
    public string Nickname { get; init; }
    
    public ulong? GuildId { get; init; }
    public GuildDto? Guild { get; init; }

    public AuthorDto(IGuildUser user)
    {
        Id = user.Id;
        Username = user.Username;
        Nickname = string.IsNullOrWhiteSpace(user.Nickname) ? user.DisplayName : user.Nickname;
        
        GuildId = user.Guild.Id;
        Guild = new GuildDto(user.Guild);
    }

    public User ToEntity()
    {
        return new User
        {
            Id = Id,
            Username = Username
        };
    }
}