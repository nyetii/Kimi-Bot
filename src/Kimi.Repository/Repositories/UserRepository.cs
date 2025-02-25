using Discord.WebSocket;
using Kimi.Repository.Dtos;
using Kimi.Repository.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;

namespace Kimi.Repository.Repositories;

public class UserRepository
{
    private readonly ILogger<UserRepository> _logger;
    private readonly KimiDbContext _dbContext;

    public UserRepository(ILogger<UserRepository> logger, KimiDbContext dbContext)
    {
        _logger = logger;
        _dbContext = dbContext;
    }

    public async Task<IDbContextTransaction> BeginTransactionAsync()
    {
        return await _dbContext.Database.BeginTransactionAsync();
    }

    public async Task<User> GetOrCreateAsync(MessageDto message)
    {
        if (message.Author is null)
            throw new Exception("Author is null.");

        var user = await _dbContext.Users
            .Include(x => x.GuildUsers)
            .FirstOrDefaultAsync(x => x.Id == message.Author.Id);

        if (user is not null)
            return user;

        user = message.Author.ToEntity();

        var guild = await _dbContext.Guilds.FindAsync(message.Author.GuildId) ?? throw new Exception("Guild is null.");

        user.Guilds.Add(guild);
        user.GuildUsers.Add(new GuildUser
        {
            GuildId = guild.Id,
            UserId = message.Author.Id,
            Nickname = message.Author.Nickname
        });

        await _dbContext.Users.AddAsync(user);

        _logger.LogInformation("Added new user {username}", user.Username);

        await _dbContext.SaveChangesAsync();

        return user;
    }

    public async Task IncrementScoreAsync(MessageDto message, uint score)
    {
        if (message.Author is not { Guild: not null })
            throw new Exception("Author is null.");

        var daily = await _dbContext.DailyScores
            .Include(x => x.Guild)
            .Include(x => x.User)
            .ThenInclude(x => x.GuildUsers)
            .Where(x => x.UserId == message.Author.Id
                        && x.GuildId == message.Author.Guild.Id)
            .FirstOrDefaultAsync(x => x.Date == DateOnly.FromDateTime(DateTime.UtcNow));

        if (daily is not null)
        {
            daily.Score += score;
            daily.MessageCount++;
            _dbContext.DailyScores.Update(daily);
            _logger.LogInformation("[{guild}] Added {score} points to {username} [Daily Total {total}]",
                message.Author.Guild.Name, score, daily.User.Username,
                daily.Score);

            await _dbContext.SaveChangesAsync();
            return;
        }

        daily = new DailyScore
        {
            Date = DateOnly.FromDateTime(DateTime.UtcNow),
            Score = score,
            MessageCount = 1,
            GuildId = message.Author.Guild.Id,
            UserId = message.Author.Id
        };
        await _dbContext.DailyScores.AddAsync(daily);
        _logger.LogInformation("[{guild}] Created today's score stats for {username}", message.Author.Guild.Name,
            message.Author.Username);

        await _dbContext.SaveChangesAsync();
    }

    public async Task DecrementScoreAsync(MessageDto message, uint score)
    {
        if (message.Author is not { Guild: not null })
            throw new Exception("Author is null.");

        var daily = await _dbContext.DailyScores
            .Include(x => x.Guild)
            .Include(x => x.User)
            .ThenInclude(x => x.GuildUsers)
            .Where(x => x.UserId == message.Author.Id
                        && x.GuildId == message.Author.Guild.Id)
            .FirstOrDefaultAsync(x => x.Date == DateOnly.FromDateTime(message.DateTime));

        if (daily is not null)
        {
            daily.Score -= score;
            daily.MessageCount--;
            _dbContext.DailyScores.Update(daily);
            _logger.LogInformation("[{guild}] Removed {score} points from {username} [Daily Total {total}]",
                message.Author.Guild.Name, score,
                daily.User.Username,
                daily.Score);

            await _dbContext.SaveChangesAsync();
            return;
        }

        _logger.LogInformation("[{guild}] {username} does not have a logged score from {date}",
            message.Author.Guild.Name, message.Author.Username,
            message.DateTime);
    }

    public async Task UpdateLastMessageAsync(MessageDto message)
    {
        if (message.Author is not { Guild: not null } userDto)
            throw new Exception("Author is null.");

        var user = await _dbContext.Users
            .Include(x => x.GuildUsers)
            .FirstOrDefaultAsync(x => x.Id == userDto.Id) ?? throw new Exception("User is null.");

        var userInfo = user.GetGuildUserInfo(userDto.Guild.Id);
        userInfo.LastMessage = message.DateTime;

        _dbContext.GuildUsers.Update(userInfo);

        _logger.LogInformation("[{guild}] Updated last message from {username}: {message}", message.Author.Guild.Name,
            user.Username, message.DateTime);

        await _dbContext.SaveChangesAsync();
    }

    public async Task UpdateNamesAsync(SocketGuildUser discordUser)
    {
        var user = await _dbContext.Users
            .Include(x => x.GuildUsers)
            .FirstOrDefaultAsync(x => x.Id == discordUser.Id);

        if (user is null)
            return;

        var guildUser = user.GetGuildUserInfo(discordUser.Guild.Id);

        if (user.Username != discordUser.Username)
        {
            _logger.LogInformation("Updated username: {old} -> {new}", user.Username, discordUser.Username);
            user.Username = discordUser.Username;
            _dbContext.Users.Update(user);
        }

        if (guildUser.Nickname != discordUser.Nickname)
        {
            _logger.LogInformation("[{guild}] Updated nickname: {old} -> {new}", discordUser.Guild.Name,
                guildUser.Nickname, discordUser.Nickname);
            guildUser.Nickname = discordUser.Nickname;
            
            _dbContext.GuildUsers.Update(guildUser);
        }

        await _dbContext.SaveChangesAsync();
    }

    public async Task<ulong[]> GetBirthdayUserIdsAsync(DateTime? dateTime = null)
    {
        var date = DateOnly.FromDateTime(
            TimeZoneInfo.ConvertTimeFromUtc(dateTime?.ToUniversalTime() ?? DateTime.UtcNow,
                TimeZoneInfo.FindSystemTimeZoneById("E. South America Standard Time")));

        return await _dbContext.Users
            .Where(x => x.Birthday != null
                        && x.Birthday.Value.Month == date.Month
                        && x.Birthday.Value.Day == date.Day)
            .AsNoTracking()
            .Select(x => x.Id)
            .ToArrayAsync();
    }


    public async Task UpdateBirthdateAsync(ulong userId, DateTime? dateTime = null)
    {
        var user = await _dbContext.Users.FindAsync(userId) ?? throw new Exception("User not found.");

        if (user.Birthday is not null && dateTime is not null)
            throw new Exception("Birthdate is already set.");

        user.Birthday = dateTime is not null
            ? DateOnly.FromDateTime(dateTime.Value.ToUniversalTime())
            : null;

        _logger.LogInformation("Updated birthdate for {user}: {date}", user.Username, user.Birthday);

        _dbContext.Users.Update(user);
        await _dbContext.SaveChangesAsync();
    }

    public async Task CommitAsync(IDbContextTransaction transaction)
    {
        await transaction.CommitAsync();
        await transaction.DisposeAsync();
    }
}