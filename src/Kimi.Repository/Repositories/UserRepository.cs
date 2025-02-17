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

        var user = await _dbContext.Users.FirstOrDefaultAsync(x => x.Id == message.Author.Id);

        if (user is not null)
            return user;

        user = message.Author.ToEntity();

        var guild = await _dbContext.Guilds.FindAsync(message.Author.GuildId) ?? throw new Exception("Guild is null.");

        user.Guilds.Add(guild);
        user.GuildsUsers.Add(new GuildUser
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
            .ThenInclude(x => x.GuildsUsers)
            .Where(x => x.UserId == message.Author.Id
                        && x.GuildId == message.Author.Guild.Id)
            .FirstOrDefaultAsync(x => x.Date == DateOnly.FromDateTime(DateTime.Today));

        if (daily is not null)
        {
            daily.Score += score;
            _dbContext.DailyScores.Update(daily);
            _logger.LogInformation("Added {score} points to {username} [Total {total}]", score, daily.User.Username, daily.Score);
            
            await _dbContext.SaveChangesAsync();
            return;
        }

        daily = new DailyScore
        {
            Date = DateOnly.FromDateTime(DateTime.Today),
            Score = score,
            GuildId = message.Author.Guild.Id,
            UserId = message.Author.Id
        };
        await _dbContext.DailyScores.AddAsync(daily);
        _logger.LogInformation("Created today's score stats for {username}", message.Author.Username);
        
        await _dbContext.SaveChangesAsync();
    }

    public async Task UpdateLastMessageAsync(MessageDto message)
    {
        if (message.Author is not { Guild: not null } userDto)
            throw new Exception("Author is null.");

        var user = await _dbContext.Users
            .Include(x => x.GuildsUsers)
            .FirstOrDefaultAsync(x => x.Id == userDto.Id) ?? throw new Exception("User is null.");

        var userInfo = user.GetGuildUserInfo(userDto.Guild.Id);
        userInfo.LastMessage = message.DateTime;

        _dbContext.GuildUsers.Update(userInfo);
        
        _logger.LogInformation("Updated last message from {username}: {message}", user.Username, message.DateTime);
        
        await _dbContext.SaveChangesAsync();
    }

    public async Task CommitAsync(IDbContextTransaction transaction)
    {
        await transaction.CommitAsync();
        await transaction.DisposeAsync();
    }
}