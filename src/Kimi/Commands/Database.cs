using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Kimi.Extensions;
using Kimi.Repository;
using Kimi.Repository.Models;
using Microsoft.EntityFrameworkCore;

namespace Kimi.Commands;

[RequireOwner]
[Group("db")]
public class Database : ModuleBase<SocketCommandContext>
{
    private readonly KimiDbContext _dbContext;

    public Database()
    {
        _dbContext = new KimiDbContext();
    }

    [Group("add")]
    public class DatabaseAdd : Database
    {
        private readonly ILogger<DatabaseAdd> _logger;

        public DatabaseAdd(ILogger<DatabaseAdd> logger)
        {
            _logger = logger;
        }

        [Command("guild")]
        public async Task AddGuildCommand()
        {
            var guild = new Guild
            {
                Id = Context.Guild.Id,
                Name = Context.Guild.Name,
                DailyScores = []
            };

            var users = (await _dbContext.Users
                .Include(x => x.Guilds)
                .Include(x => x.DailyScores)
                .Include(x => x.GuildsUsers)
                .ToListAsync())
                .Join(Context.Guild.Users.Where(x => !x.IsBot), dbUser => dbUser.Id, guildUser => guildUser.Id,
                (dbUser, guildUser) =>
                new User
                {
                    Id = dbUser.Id,
                    Username = guildUser.Username,
                    Guilds = dbUser.Guilds,
                    DailyScores = dbUser.DailyScores,
                    GuildsUsers = dbUser.GuildsUsers
                }).ToList();

            users.AddRange(Context.Guild.Users
                .Where(x => !x.IsBot)
                .Select(user => new User
                {
                    Id = user.Id,
                    Username = user.Username,
                    Guilds = [],
                    DailyScores = [],
                }));

            var nicknames = Context.Guild.Users.Select(x => (x.Id, x.Nickname)).ToArray();

            foreach (var user in users)
            {
                user.Guilds.Add(guild);
                user.GuildsUsers.Add(new GuildUser
                {
                    GuildId = guild.Id,
                    UserId = user.Id,
                    Nickname = nicknames.FirstOrDefault(x => x.Id == user.Id).Nickname
                });
            }

            var newUsers = users.Where(x => x.Guilds.Count is 1).ToArray();
            var existingUsers = users.Where(x => x.Guilds.Count > 1).ToArray();

            try
            {
                await _dbContext.Users.AddRangeAsync(newUsers);

                if (existingUsers.Length > 1)
                    _dbContext.Users.UpdateRange(existingUsers);

                await _dbContext.Guilds.AddAsync(guild);
                await _dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{guildName} [{guildId}] could not be added.", guild.Name, guild.Id);
                throw;
            }

            await Context.Message.ReplyAsync("Added guild.");
        }
    }

    [Group("delete")]
    public class DatabaseDelete : Database
    {
        private readonly ILogger<DatabaseDelete> _logger;

        public DatabaseDelete(ILogger<DatabaseDelete> logger)
        {
            _logger = logger;
        }

        [Command("guild")]
        public async Task DeleteGuildCommand()
        {
            var guild = await _dbContext.Guilds
                .Include(x => x.DailyScores)
                .Include(x => x.GuildUsers)
                .FirstOrDefaultAsync(x => x.Id == Context.Guild.Id);

            if (guild is null)
            {
                await Context.Message.ReplyAsync("Guild does not exist.");
                return;
            }

            var users = _dbContext.Users.Include(x => x.Guilds).Where(x => x.Guilds.All(g => g.Id == guild.Id));

            try
            {
                _dbContext.Remove(guild);
                _dbContext.RemoveRange(guild.GuildUsers);
                _dbContext.RemoveRange(users);
                _dbContext.RemoveRange(guild.DailyScores);
                await _dbContext.SaveChangesAsync();

                _logger.LogInformation("Deleted {guildName} [{guildId}]", guild.Name, guild.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{guildName} [{guildId}] could not be added.", guild.Name, guild.Id);
                throw;
            }

            await Context.Message.ReplyAsync("Deleted guild.");
        }


        [Command("user")]
        public async Task DeleteUsersCommand([Remainder] string user)
        {
            if (user is "all")
            {
                _dbContext.Users.RemoveRange(_dbContext.Users);

                await _dbContext.SaveChangesAsync();

                _logger.LogWarning("All users were deleted.");
                await Context.Message.ReplyAsync("Deleted all users");
                return;
            }
            
            if (user is "guild")
            {
                var users = _dbContext.Users.Include(x => x.Guilds)
                    .Where(x => x.Guilds.All(g => g.Id == Context.Guild.Id));
                _dbContext.Users.RemoveRange(users);

                await _dbContext.SaveChangesAsync();

                _logger.LogWarning("All users from {guildName} [{guildId}] were deleted.", Context.Guild.Name, Context.Guild.Id);
                await Context.Message.ReplyAsync("All users from the guild were deleted");
                return;
            }
            
            if (ulong.TryParse(user, out var id))
            {
                var userEntity = await _dbContext.Users.FindAsync(id);

                if (userEntity is null)
                {
                    await Context.Message.ReplyAsync("User not found.");
                    return;
                }

                _dbContext.Users.Remove(userEntity);
            }
            else
            {
                var userEntity = await _dbContext.Users.FirstOrDefaultAsync(x => x.Username == user);

                if (userEntity is null)
                {
                    await Context.Message.ReplyAsync("User not found.");
                    return;
                }

                _dbContext.Users.Remove(userEntity);
            }

            await Context.Message.ReplyAsync("Deleted user successfully.");
        }
    }

    [Group("log")]
    public class DatabaseLog : Database
    {
        private readonly ILogger<DatabaseLog> _logger;

        private readonly DiscordSocketClient _client;

        public DatabaseLog(ILogger<DatabaseLog> logger, DiscordSocketClient client)
        {
            _logger = logger;
            _client = client;
        }

        [Command("user")]
        public async Task LogUser(string? user)
        {
            user ??= Context.User.Username;

            User? userEntity;

            if (user.TryParseHandle(out user) && ulong.TryParse(user, out var id))
            {
                userEntity = await _dbContext.Users
                    .Include(x => x.GuildsUsers)
                    .Include(x => x.DailyScores)
                    .Where(x => x.GuildsUsers.Any(g => g.GuildId == Context.Guild.Id))
                    .FirstOrDefaultAsync(x => x.Id == id);
            }
            else
            {
                userEntity = await _dbContext.Users
                    .Include(x => x.GuildsUsers)
                    .Include(x => x.DailyScores)
                    .Where(x => x.GuildsUsers.Any(g => g.GuildId == Context.Guild.Id))
                    .FirstOrDefaultAsync(x => x.Username == user);
            }

            if (userEntity is null)
            {
                await Context.Message.ReplyAsync("User not found.");
                return;
            }

            var guildInfo = userEntity.GetGuildUserInfo(Context.Guild.Id);
            var score = userEntity.DailyScores.Sum(x => x.Score);
            _logger.LogInformation("[{userId}] {userName} ({userNickname}) - {score} points, last message sent at {lastMessage}",
                userEntity.Id, userEntity.Username, guildInfo.Nickname, score, guildInfo.LastMessage);
        }

        [Command("users")]
        public async Task LogUsers(ulong? guildId = null)
        {
            SocketGuild guild;

            if (guildId is not null)
                guild = _client.Guilds.FirstOrDefault(x => x.Id == Convert.ToUInt64(guildId)) ?? throw new Exception("Invalid guild.");
            else
                guild = Context.Guild;

            var users = await _dbContext.Users
                .Include(x => x.GuildsUsers)
                .Include(x => x.DailyScores)
                .Where(x => x.GuildsUsers.Any(guildUser => guildUser.GuildId == guild.Id))
                .ToListAsync();

            _logger.LogInformation("{guildName} Users ({userCount} users)", guild.Name, guild.Users.Count);
            foreach (var user in users)
            {
                var guildInfo = user.GetGuildUserInfo(Context.Guild.Id);
                var score = user.DailyScores.Sum(x => x.Score);
                _logger.LogInformation("[{userId}] {userName} ({userNickname}) - {score} points, last message sent at {lastMessage}",
                    user.Id, user.Username, guildInfo.Nickname, score, guildInfo.LastMessage);
            }
        }
    }
}