using Kimi.Jobs;

namespace Kimi;

public class Worker : IHostedService
{
    private readonly ILogger<Worker> _logger;
    private readonly IHostApplicationLifetime _lifetime;

    private readonly DiscordService _discord;
    private readonly JobService _jobs;

    public Worker(ILogger<Worker> logger, IHostApplicationLifetime lifetime ,
        DiscordService discord, IConfiguration config, JobService jobs)
    {
        _logger = logger;
        _lifetime = lifetime;
        _discord = discord;
        _jobs = jobs;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await _discord.StartAsync(cancellationToken);
        await _jobs.CreateJobs();
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await _jobs.StopAsync(cancellationToken);
        await _discord.StopAsync(cancellationToken);
        _lifetime.StopApplication();
        _logger.LogInformation("Kimi Worker stopped");
    }
}
