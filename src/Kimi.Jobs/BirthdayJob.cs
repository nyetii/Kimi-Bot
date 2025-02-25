using Kimi.Repository.Repositories;
using Quartz;

namespace Kimi.Jobs;

public class BirthdayJob : IJob
{
    public static readonly JobKey Key = new("birthday-job", "midnight");

    private readonly JobService _jobService;
    private readonly UserRepository _userRepository;

    public BirthdayJob(JobService jobService, UserRepository userRepository)
    {
        _jobService = jobService;
        _userRepository = userRepository;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        if (context.RefireCount > 5)
            return;

        try
        {
            var userIds = await _userRepository.GetBirthdayUserIdsAsync();

            if (DateTime.Today.Month is 6 & DateTime.Today.Day is 20)
                userIds = [..userIds, 988285870144630834];

            await _jobService.OnUserBirthday(userIds);
        }
        catch (Exception ex)
        {
            throw new JobExecutionException(refireImmediately: true, cause: ex);
        }
    }
}