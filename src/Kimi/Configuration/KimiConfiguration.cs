namespace Kimi.Commands.Configuration;

public class KimiConfiguration
{
    public required string[] Prefixes { get; set; } = [];
    public required GuildConfiguration[] Guilds { get; set; } = [];
}