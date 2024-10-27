using Discord;
using Discord.WebSocket;
using Kimi.Commands;
using Kimi.Modules.Ranking;

namespace Kimi;

public class Worker : IHostedService
{
    private readonly ILogger<Worker> _logger;

    private readonly string _token;

    private readonly DiscordSocketClient _client;
    private readonly CommandHandler _command;
    private readonly RankingService _rankingService;

    public Worker(ILogger<Worker> logger, IConfiguration config, IServiceProvider provider)
    {
        _logger = logger;
        _token = config.GetSection("Discord")["Token"] ?? throw new InvalidOperationException("Could not get token.");
        _client = provider.GetRequiredService<DiscordSocketClient>();
        _command = provider.GetRequiredService<CommandHandler>();
        _rankingService = provider.GetRequiredService<RankingService>();
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
        _client.Log += OnLogEvent;

        await _command.InitializeAsync();
        await _rankingService.InitializeAsync();

        _client.Ready += async () =>
        {

        };

        await _client.LoginAsync(TokenType.Bot, _token);

        await _client.StartAsync();
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await _client.LogoutAsync();
    }

    private async Task OnLogEvent(LogMessage message)
    {
        var level = message.Severity switch
        {
            LogSeverity.Critical => LogLevel.Critical,
            LogSeverity.Error => LogLevel.Error,
            LogSeverity.Warning => LogLevel.Warning,
            LogSeverity.Info => LogLevel.Information,
            LogSeverity.Verbose => LogLevel.Trace,
            LogSeverity.Debug => LogLevel.Debug,
            _ => LogLevel.None
        };

        _logger.Log(level, message.Exception, message.Message);

        await Task.CompletedTask;
    }
}
