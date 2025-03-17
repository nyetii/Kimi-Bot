using Kimi.Repository.Models;
using Microsoft.Extensions.Logging;
using Quartz;

namespace Kimi.Jobs;

public sealed class JobService
{
    private readonly ILogger<JobService> _logger;
    private readonly ISchedulerFactory _schedulerFactory;

    public JobService(ILogger<JobService> logger, ISchedulerFactory schedulerFactory)
    {
        _schedulerFactory = schedulerFactory;
        _logger = logger;
    }

    public event UserBirthdayHandler? UserBirthday;
    public delegate Task UserBirthdayHandler(object sender, ulong[] userIds);

    internal async Task OnUserBirthday(ulong[] userIds)
    {
        UserBirthday?.Invoke(this, userIds);
        await Task.CompletedTask;
    }
    
    public event RankingUpdateHandler? RankingUpdate;
    public delegate Task RankingUpdateHandler(Dictionary<Guild, DailyScore[]> guilds);
    
    internal async Task OnRankingUpdate(Dictionary<Guild, DailyScore[]> guilds)
    {
        RankingUpdate?.Invoke(guilds);
        await Task.CompletedTask;
    }
    
    public async Task CreateJobs()
    {
        var scheduler = await _schedulerFactory.GetScheduler();

        var birthdayJob = JobBuilder.Create<BirthdayJob>()
            .WithIdentity(BirthdayJob.Key)
            .Build();
        
        var scoreJob = JobBuilder.Create<ScoreJob>()
            .WithIdentity(ScoreJob.Key)
            .Build();
        
        var backupJob = JobBuilder.Create<BackupJob>()
            .WithIdentity(BackupJob.Key)
            .Build();

        var birthdayTrigger = TriggerBuilder.Create()
            .WithIdentity("birthday-trigger")
            .ForJob(birthdayJob)
            .StartNow()
            .WithCronSchedule("0 0 0 1/1 * ? *",
                x => x
                    .InTimeZone(TimeZoneInfo.FindSystemTimeZoneById("E. South America Standard Time"))
                    .WithMisfireHandlingInstructionFireAndProceed())
            .Build();
        
        var scoreTrigger = TriggerBuilder.Create()
            .WithIdentity("score-trigger")
            .ForJob(scoreJob)
            .StartNow()
            .WithCronSchedule("0 0 0/1 1/1 * ? *",
                x => x
                    .InTimeZone(TimeZoneInfo.FindSystemTimeZoneById("E. South America Standard Time"))
                    .WithMisfireHandlingInstructionFireAndProceed())
            .Build();
        
        var backupTrigger = TriggerBuilder.Create()
            .WithIdentity("backup-trigger")
            .ForJob(backupJob)
            .StartNow()
            .WithCronSchedule("0 0 0 1/1 * ? *",
                x => x
                    .InTimeZone(TimeZoneInfo.FindSystemTimeZoneById("E. South America Standard Time"))
                    .WithMisfireHandlingInstructionFireAndProceed())
            .Build();

        await scheduler.ScheduleJob(birthdayJob, birthdayTrigger);
        await scheduler.ScheduleJob(scoreJob, scoreTrigger);
        await scheduler.ScheduleJob(backupJob, backupTrigger);
        await scheduler.Start();

        await scheduler.TriggerJob(birthdayJob.Key);
        await scheduler.TriggerJob(scoreJob.Key);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        try
        {
            var scheduler = await _schedulerFactory.GetScheduler(cancellationToken);
            await scheduler.Shutdown(true, cancellationToken);
        }
        catch (Exception ex) when (!ex.Message.Contains("shutdown", StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogError(ex, "Error while shutting down the scheduler.");
        }
        catch (Exception)
        {
            // Ignored
        }
    }

    public async Task ForceTriggerAsync(string jobName)
    {
        var scheduler = await _schedulerFactory.GetScheduler();
        await scheduler.TriggerJob(new JobKey(jobName));
    }
}