using Discord;
using Discord.WebSocket;
using Kimi.Commands;
using Kimi.Modules.Ranking;
using Kimi.Repository.Dtos;
using Kimi.Repository.Repositories;

namespace Kimi;

public class DiscordService
{
    private readonly ILogger<DiscordService> _logger;

    private readonly string _token;

    private readonly DiscordSocketClient _client;
    private readonly CommandHandler _command;
    private readonly InteractionHandler _interaction;

    private readonly RankingService _rankingService;

    private readonly UserRepository _userRepository;
    private readonly GuildRepository _guildRepository;

    private readonly ulong[] _guilds;

    public DiscordService(ILogger<DiscordService> logger, DiscordSocketClient client,
        CommandHandler command, InteractionHandler interaction, IServiceScopeFactory scopeFactory,
        IConfiguration config)
    {
        _logger = logger;
        _token = config["Discord:Token"] ?? throw new Exception("Token is missing.");
        _client = client;
        _command = command;
        _interaction = interaction;

        var scope = scopeFactory.CreateScope();
        _userRepository = scope.ServiceProvider.GetRequiredService<UserRepository>();
        _guildRepository = scope.ServiceProvider.GetRequiredService<GuildRepository>();
        _rankingService = scope.ServiceProvider.GetRequiredService<RankingService>();

        _guilds = config.GetSection("Kimi:Ranking:Guilds").Get<ulong[]>() ?? [];
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _client.Log += OnLogEvent;

        await _command.InitializeAsync();
        await _interaction.InitializeAsync();

        _client.Ready += OnReadyAsync;
    
        await _client.LoginAsync(TokenType.Bot, _token);

        await _client.StartAsync();
    }

    private async Task OnReadyAsync()
    {
        var guilds = _client.Guilds.Where(x => _guilds.Contains(x.Id))
            .Select(x => new GuildDto(x)).ToArray();

        foreach (var guild in guilds)
            await _guildRepository.GetOrCreateAsync(guild);

        await _guildRepository.SaveAsync();

        await _rankingService.InitializeAsync();
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