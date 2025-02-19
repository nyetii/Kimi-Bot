namespace Kimi;

public class Worker : IHostedService
{
    private readonly ILogger<Worker> _logger;

    private readonly DiscordService _discord;

    public Worker(ILogger<Worker> logger, DiscordService discord, IConfiguration config)
    {
        _logger = logger;
        _discord = discord;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await _discord.StartAsync(cancellationToken);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await _discord.StopAsync(cancellationToken);
    }
}
