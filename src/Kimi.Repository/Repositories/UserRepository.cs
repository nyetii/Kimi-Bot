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

    public async Task<User> GetOrCreateAsync(AuthorDto
        author, bool searchForMain = true)
    {
        if (author is null)
            throw new Exception("Author is null.");

        var user = await FindUserAsync(author.Id, searchForMain);

        if (user is not null)
            return user;

        user = author.ToEntity();

        var guild = await _dbContext.Guilds.FindAsync(author.GuildId) ?? throw new Exception("Guild is null.");

        user.Guilds.Add(guild);
        user.GuildUsers.Add(new GuildUser
        {
            GuildId = guild.Id,
            UserId = author.Id,
            Nickname = author.Nickname
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

        // SQLite UPSERT to handle concurrency atomically
        var date = DateOnly.FromDateTime(DateTime.UtcNow);

        await _dbContext.Database.ExecuteSqlInterpolatedAsync($"""
                                                               
                                                                       INSERT INTO DailyScores (UserId, GuildId, Date, Score, MessageCount)
                                                                       VALUES ({message.Author.Id}, {message.Author.Guild.Id}, {date}, {score}, 1)
                                                                       ON CONFLICT(UserId, GuildId, Date)
                                                                       DO UPDATE SET
                                                                           Score = Score + {score},
                                                                           MessageCount = MessageCount + 1;
                                                                   
                                                               """);
        _logger.LogInformation("[{guild}] Created today's score stats for {username}", message.Author.Guild.Name,
            message.Author.Username);
    }

    public async Task DecrementScoreAsync(MessageDto message, uint score)
    {
        if (message.Author is not { Guild: not null })
            throw new Exception("Author is null.");

        var user = await FindUserAsync(message.Author.Id) ?? throw new Exception("User is null.");

        var daily = await _dbContext.DailyScores
            .Include(x => x.Guild)
            .Include(x => x.User)
            .ThenInclude(x => x.GuildUsers)
            .Where(x => x.UserId == user.Id
                        && x.GuildId == message.Author.Guild.Id)
            .FirstOrDefaultAsync(x => x.Date == DateOnly.FromDateTime(message.DateTime));

        if (daily is not null)
        {
            daily.Score = daily.Score > score
                ? daily.Score - score
                : 0;
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

        var user = await FindUserAsync(userDto.Id) ?? throw new Exception("User is null.");

        var userInfo = user.GetGuildUserInfo(userDto.Guild.Id);
        userInfo.LastMessage = message.DateTime;

        _dbContext.GuildUsers.Update(userInfo);

        _logger.LogInformation("[{guild}] Updated last message from {username}: {message}", message.Author.Guild.Name,
            user.Username, message.DateTime);

        await _dbContext.SaveChangesAsync();
    }

    public async Task UpdateNamesAsync(SocketGuildUser discordUser)
    {
        var user = await FindUserAsync(discordUser.Id);

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
            guildUser.Nickname = string.IsNullOrWhiteSpace(discordUser.Nickname)
                ? discordUser.DisplayName
                : discordUser.Nickname;

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
            .Where(x => !x.Hidden
                        && x.Birthday != null
                        && x.Birthday.Value.Month == date.Month
                        && x.Birthday.Value.Day == date.Day)
            .AsNoTracking()
            .Select(x => x.Id)
            .ToArrayAsync();
    }


    public async Task UpdateBirthdateAsync(ulong userId, DateTime? dateTime = null)
    {
        var user = await FindUserAsync(userId) ?? throw new Exception("User not found.");

        if (user.Birthday is not null && dateTime is not null)
            throw new Exception("Birthdate is already set.");

        user.Birthday = dateTime is not null
            ? DateOnly.FromDateTime(dateTime.Value.ToUniversalTime())
            : null;

        _logger.LogInformation("Updated birthdate for {user}: {date}", user.Username, user.Birthday);

        _dbContext.Users.Update(user);
        await _dbContext.SaveChangesAsync();
    }

    public void AsMainUser(ref User user)
    {
        var id = user.MainUserId;

        if (id.HasValue)
            user = user.MainUser ?? throw new Exception("Main user is null. Has it been included in the query?");
    }

    public async Task<User?> FindUserAsync(ulong userId, bool searchForMain = true)
    {
        var user = await _dbContext.Users
            .Include(x => x.GuildUsers)
            .Include(x => x.MainUser)
            .ThenInclude(x => x!.GuildUsers)
            .FirstOrDefaultAsync(x => x.Id == userId);

        if (user is null)
            return null;

        if (user.MainUserId.HasValue && searchForMain)
            user = user.MainUser ?? throw new Exception("Main user is null, even though an Id is set.");

        return user;
    }

    public async Task LinkUsersAsync(ulong mainUserId, ulong secondaryUserId)
    {
        var mainUser = await _dbContext.Users
            .Include(x => x.GuildUsers)
            .FirstOrDefaultAsync(x => x.Id == mainUserId);

        var secondaryUser = await _dbContext.Users
            .Include(x => x.GuildUsers)
            .FirstOrDefaultAsync(x => x.Id == secondaryUserId);

        if (mainUser is null || secondaryUser is null)
            throw new Exception("Could not find user to link.");

        mainUser.MainUserId = null;

        secondaryUser.MainUserId = mainUserId;
        secondaryUser.MainUser = mainUser;

        _dbContext.Users.UpdateRange(mainUser, secondaryUser);
        await _dbContext.SaveChangesAsync();
    }

    // public async Task SyncUserAsync(ulong userId)
    // {
    //     var mainUser = await _dbContext.Users
    //         .Include(x => x.GuildUsers)
    //         .Include(x => x.DailyScores)
    //         .Include(x => x.MainUser)
    //         .FirstOrDefaultAsync(x => x.Id == userId);
    //
    //     if (mainUser is null)
    //         throw new Exception("Could not find user.");
    //
    //     if (mainUser.MainUserId.HasValue)
    //         mainUser = mainUser.MainUser ?? throw new Exception("Main user is null, even though an Id is set.");
    //
    //     var secondaryUsers = await _dbContext.Users
    //         .Include(x => x.GuildUsers)
    //         .Include(x => x.DailyScores)
    //         .Where(x => x.MainUserId == userId)
    //         .ToListAsync();
    //
    //     var secondaryScores = secondaryUsers.SelectMany(x => x.DailyScores).GroupBy(x => x.Date);
    //
    //     foreach (var score in mainUser.DailyScores)
    //     {
    //         score.Score = 
    //     }
    // }

    public async Task CommitAsync(IDbContextTransaction transaction)
    {
        await transaction.CommitAsync();
        await transaction.DisposeAsync();
    }
}