using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Kimi.Repository.Models;

[PrimaryKey(nameof(GuildId), nameof(UserId))]
[Index(nameof(GuildId), nameof(UserId), IsUnique = true)]
public class GuildUser
{
    public ulong GuildId { get; init; }
    public Guild Guild { get; init; } = null!;

    public ulong UserId { get; init; }
    public User User { get; init; } = null!;

    [MaxLength(32)]
    public string? Nickname { get; set; }
    public DateTime? LastMessage { get; set; }
}