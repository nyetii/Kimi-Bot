namespace Kimi;

public class Worker : IHostedService
{
    private readonly ILogger<Worker> _logger;
    private readonly IServiceScopeFactory _scopeFactory;

    private readonly DiscordService _discord;

    private readonly ulong[] _guilds;

    public Worker(ILogger<Worker> logger, IServiceScopeFactory scopeFactory, DiscordService discord, IConfiguration config)
    {
        _logger = logger;
        _scopeFactory = scopeFactory;
        _discord = discord;
        _guilds = config.GetSection("Kimi:Ranking:Guilds").Get<ulong[]>() ?? [];
    }

    protected async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
            }
            await Task.Delay(1000, stoppingToken);
        }
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
