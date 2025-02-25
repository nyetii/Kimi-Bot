using Kimi.Repository.Models;
using Quartz;

namespace Kimi.Jobs;

public class JobService
{
    private readonly ISchedulerFactory _schedulerFactory;

    public JobService(ISchedulerFactory schedulerFactory)
    {
        _schedulerFactory = schedulerFactory;
    }

    public event UserBirthdayHandler? UserBirthday;
    public delegate Task UserBirthdayHandler(object sender, ulong[] userIds);

    internal virtual async Task OnUserBirthday(ulong[] userIds)
    {
        UserBirthday?.Invoke(this, userIds);
        await Task.CompletedTask;
    }
    
    public event RankingUpdateHandler? RankingUpdate;
    public delegate Task RankingUpdateHandler(Dictionary<Guild, DailyScore[]> guilds);
    
    internal virtual async Task OnRankingUpdate(Dictionary<Guild, DailyScore[]> guilds)
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

        var birthdayTrigger = TriggerBuilder.Create()
            .WithIdentity("birthday-trigger", "midnight")
            .ForJob(birthdayJob)
            .StartNow()
            .WithCronSchedule("0 0 0 1/1 * ? *",
                x => x
                    .InTimeZone(TimeZoneInfo.FindSystemTimeZoneById("E. South America Standard Time"))
                    .WithMisfireHandlingInstructionFireAndProceed())
            .Build();
        
        var scoreTrigger = TriggerBuilder.Create()
            .WithIdentity("score-trigger", "hourly")
            .ForJob(scoreJob)
            .StartNow()
            .WithCronSchedule("0 0 0/1 1/1 * ? *",
                x => x
                    .InTimeZone(TimeZoneInfo.FindSystemTimeZoneById("E. South America Standard Time"))
                    .WithMisfireHandlingInstructionFireAndProceed())
            .Build();

        await scheduler.ScheduleJob(birthdayJob, birthdayTrigger);
        await scheduler.ScheduleJob(scoreJob, scoreTrigger);
        await scheduler.Start();

        await Task.Delay(10000);

        await scheduler.TriggerJob(birthdayJob.Key);
        await scheduler.TriggerJob(scoreJob.Key);
    }
}