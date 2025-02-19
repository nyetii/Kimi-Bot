namespace Kimi.Commands.Configuration;

public class GuildConfiguration
{
    public required ulong Id { get; init; }
    public required string Name { get; set; }
    public required Dictionary<string, bool> Modules { get; set; } = [];
    public required ulong[] RoleIds { get; set; } = [];
    public ulong? BirthdayRoleId { get; set; }
}