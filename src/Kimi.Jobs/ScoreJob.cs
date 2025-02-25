using Kimi.Repository.Repositories;
using Microsoft.Extensions.Logging;
using Quartz;

namespace Kimi.Jobs;

public class ScoreJob : IJob
{
    public static readonly JobKey Key = new("score-job", "hourly");
    
    private readonly ILogger<ScoreJob> _logger;
    private readonly JobService _jobService;
    
    private readonly GuildRepository _guildRepository;

    public ScoreJob(ILogger<ScoreJob> logger, JobService jobService, GuildRepository guildRepository)
    {
        _logger = logger;
        _jobService = jobService;
        _guildRepository = guildRepository;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        if (context.RefireCount > 5)
            return;
        
        try
        {
            var guilds = await _guildRepository.GetScoresFromAllGuildsAsync(DateTime.Now.AddDays(-1));

            await _jobService.OnRankingUpdate(guilds);
        }
        catch (Exception ex)
        {
            throw new JobExecutionException(refireImmediately: true, cause: ex);
        }
    }
}