using Discord;
using Discord.WebSocket;
using Kimi.Configuration;
using Kimi.Jobs;
using Microsoft.Extensions.Options;

namespace Kimi.Modules.Birthday;

public class BirthdayService
{
    private readonly ILogger<BirthdayService> _logger;
    private readonly KimiConfiguration _configuration;

    private readonly JobService _jobService;
    private readonly DiscordSocketClient _client;

    public BirthdayService(ILogger<BirthdayService> logger, IOptions<KimiConfiguration> options, JobService jobService,
        DiscordSocketClient client)
    {
        _logger = logger;
        _configuration = options.Value;
        _jobService = jobService;
        _client = client;
    }

    public async Task InitializeAsync()
    {
        _logger.LogInformation("Initializing Birthday Service");
        _jobService.UserBirthday += OnUserBirthday;
        await Task.CompletedTask;
    }

    private async Task OnUserBirthday(object sender, ulong[] userIds)
    {
        var guildConfigs = _configuration.Guilds.Where(x => x.Modules["birthday"]);

        foreach (var guildConfig in guildConfigs)
        {
            var guild = _client.GetGuild(guildConfig.Id);
            var users = guild.Users.Where(x => userIds.Contains(x.Id));

            if (guildConfig.BirthdayRoleId is null)
                continue;

            var roleId = guildConfig.BirthdayRoleId.Value;

            foreach (var user in users)
                await AddBirthdayRole(user, roleId);

            var usersWithRole = guild.Users
                .Where(x => !userIds.Contains(x.Id) && x.Roles.Any(role => role.Id == roleId));

            foreach (var user in usersWithRole)
                await RemoveBirthdayRole(user, roleId);
        }
    }

    private async Task AddBirthdayRole(SocketGuildUser user, ulong roleId)
    {
        if (user.Id is 182246084117135360)
        {
            await user.RemoveRoleAsync(roleId, new RequestOptions { AuditLogReason = "Feliz aniversário!!!!" });
            return;
        }

        await user.AddRoleAsync(roleId, new RequestOptions { AuditLogReason = "User birthday." });
    }

    private async Task RemoveBirthdayRole(SocketGuildUser user, ulong roleId)
    {
        if (user.Id is 182246084117135360)
        {
            if (user.Roles.Any(x => x.Id == roleId))
                return;

            await user.AddRoleAsync(roleId, new RequestOptions { AuditLogReason = "Feliz aniversário!!!!" });
            return;
        }

        await user.RemoveRoleAsync(roleId, new RequestOptions { AuditLogReason = "User birthday is over." });
    }
}