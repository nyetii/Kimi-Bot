using System.ComponentModel.DataAnnotations;
using Discord;
using Microsoft.EntityFrameworkCore;

namespace Kimi.Repository.Models;

[Index(nameof(Default))]
public class Profile
{
    [Key]
    public int Id { get; set; }

    public bool Default { get; set; }
    
    [MaxLength(256)]
    public string StatusMessage { get; set; } = string.Empty;
    
    [MaxLength(256)]
    public string? StatusUrl { get; set; }
    public UserStatus StatusType { get; set; } = UserStatus.Online;
    public ActivityType StatusActivityType { get; set; } = ActivityType.CustomStatus;
}