using System.Collections.Immutable;
using Kimi.Repository.Dtos;
using Kimi.Repository.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Kimi.Repository.Repositories;

public class GuildRepository
{
    private readonly ILogger<GuildRepository> _logger;
    private readonly KimiDbContext _dbContext;

    public GuildRepository(ILogger<GuildRepository> logger, KimiDbContext dbContext)
    {
        _logger = logger;
        _dbContext = dbContext;
    }

    public async Task<Guild> GetOrCreateAsync(GuildDto guildDto)
    {
        var guild = await _dbContext.Guilds.FindAsync(guildDto.Id);

        if (guild is not null)
            return guild;

        guild = new Guild
        {
            Id = guildDto.Id,
            Name = guildDto.Name
        };

        await _dbContext.Guilds.AddAsync(guild);
        _logger.LogInformation("Added Guild {name} to database", guild.Name);
        return guild;
    }

    public async Task<ImmutableList<UserScoreDto>> GetAllTotalScoresAsync(ulong guildId)
    {
        var guild = await _dbContext.Guilds
                        .Include(x => x.GuildUsers)
                        .ThenInclude(x => x.User)
                        .Include(x => x.DailyScores)
                        .AsSplitQuery()
                        .AsNoTracking()
                        .FirstOrDefaultAsync(x => x.Id == guildId) ??
                    throw new Exception("Guild not found.");

        var dailyScoresByUser = guild.GuildUsers.Select(x => new
        {
            Id = x.UserId,
            Nickname = x.Nickname ?? x.User.Username,
            DailyScores = guild.DailyScores.Where(ds => ds.UserId == x.UserId)
        });
        
        var totalDailyScoresByUser = new List<UserScoreDto>();

        foreach (var user in dailyScoresByUser)
        {
            var score = (uint)user.DailyScores.Sum(x => x.Score);
            var messageCount = (uint)user.DailyScores.Sum(x => x.MessageCount);
            var dto = new UserScoreDto(user.Id, user.Nickname, score, messageCount);
            totalDailyScoresByUser.Add(dto);
        }

        return totalDailyScoresByUser.OrderByDescending(x => x.Score).ToImmutableList();
    }

    public async Task<bool> SaveAsync()
    {
        return await _dbContext.SaveChangesAsync() > 0;
    }
}