using Discord;
using Discord.WebSocket;
using Kimi.Configuration;
using Kimi.Jobs;
using Kimi.Repository.Models;
using Microsoft.Extensions.Options;

namespace Kimi.Modules.Ranking;

public class LevelService
{
    private readonly KimiConfiguration _configuration;
    private readonly ILogger<LevelService> _logger;

    private readonly DiscordSocketClient _client;

    private readonly JobService _jobService;

    public LevelService(IOptions<KimiConfiguration> configuration, ILogger<LevelService> logger, JobService jobService,
        DiscordSocketClient client)
    {
        _configuration = configuration.Value;
        _logger = logger;
        _jobService = jobService;
        _client = client;
    }

    public async Task InitializeAsync()
    {
        _logger.LogInformation("Initializing Level Service");
        _jobService.RankingUpdate += OnRankingUpdate;

        await Task.CompletedTask;
    }

    private async Task OnRankingUpdate(Dictionary<Guild, DailyScore[]> guilds)
    {
        foreach (var guild in guilds)
        {
            var conf = _configuration.Guilds.First(x => x.Id == guild.Key.Id);

            if (conf.RankingRoles is null)
                continue;

            var socketGuild = _client.GetGuild(guild.Key.Id);

            var scoreGroups = guild.Value
                .Where(x => x.GuildId == guild.Key.Id)
                .GroupBy(x => x.UserId).ToList();

            var guildUsers = socketGuild.Users
                .Where(x => guild.Value.Any(ds => ds.UserId == x.Id)).ToList();

            var users = guildUsers.Join(scoreGroups,
                gu => gu.Id, sg => sg.Key,
                (gu, sg) => new
                {
                    GuildUser = gu,
                    DailyScores = sg.ToList(),
                    RoleId = conf.RankingRoles.OrderBy(x => x.Value)
                        .LastOrDefault(x => x.Value <= sg.Sum(ds => ds.Score)).Key
                }).ToList();

            foreach (var user in users)
            {
                if (user.RoleId is not 0 && user.GuildUser.Roles.All(x => x.Id != user.RoleId))
                {
                    var role = socketGuild.Roles.First(x => x.Id == user.RoleId);
                    _logger.LogInformation("[{guild}] {username} has achieved {role}", guild.Key.Name, user.GuildUser
                        .Username, role);
                    
                    await user.GuildUser.AddRoleAsync(role,
                        new RequestOptions { AuditLogReason = "Member reached the score" });
                }

                var toBeRemoved = conf.RankingRoles
                    .Where(x => user.GuildUser.Roles
                                    .Any(u => u.Id == x.Key)
                                && x.Key != user.RoleId
                                && x.Value > user.DailyScores.Sum(ds => ds.Score))
                    .ToDictionary().Keys;

                if (toBeRemoved.Count is 0)
                    continue;

                var roles = socketGuild.Roles.Where(x => toBeRemoved.Contains(x.Id));
                _logger.LogInformation("[{guild}] {username} has lost {roles}", guild.Key.Name, user.GuildUser
                    .Username, string.Join(", ", roles));
                
                await user.GuildUser.RemoveRolesAsync(toBeRemoved,
                    new RequestOptions { AuditLogReason = "Member doesn't have the required score" });
            }
        }
    }
}