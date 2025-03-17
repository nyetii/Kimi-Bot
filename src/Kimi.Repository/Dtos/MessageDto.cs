using Discord;
using Discord.WebSocket;

namespace Kimi.Repository.Dtos;

public record MessageDto
{
    public ulong Id { get; init; }
    public string Message { get; init; } = string.Empty;
    
    public DateTime DateTime { get; init; }
    
    public ulong? AuthorId { get; init; }
    public AuthorDto? Author { get; init; }
    
    public ulong? ChannelId { get; init; }

    public MessageDto(IMessage message)
    {
        Id = message.Id;
        Message = message.Content;
        
        DateTime = message.Timestamp.UtcDateTime;
        
        AuthorId = message.Author.Id;
        Author = message.Author is IGuildUser guildUser ? new AuthorDto(guildUser) : null;
        
        ChannelId = message.Channel.Id;
    }
}