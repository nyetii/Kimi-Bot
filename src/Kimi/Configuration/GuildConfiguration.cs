namespace Kimi.Configuration;

public class GuildConfiguration
{
    public required ulong Id { get; init; }
    public required string Name { get; set; }
    public required Dictionary<string, bool> Modules { get; set; } = [];
    public required Dictionary<ulong, int>? RankingRoles { get; set; } = null;
    public ulong? BirthdayRoleId { get; set; }
}