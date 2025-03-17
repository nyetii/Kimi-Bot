using Discord;
using Discord.WebSocket;
using Kimi.Commands;
using Kimi.Configuration;
using Kimi.Modules.Birthday;
using Kimi.Modules.Ranking;
using Kimi.Repository.Dtos;
using Kimi.Repository.Repositories;
using Microsoft.Extensions.Options;

namespace Kimi;

public class DiscordService
{
    private readonly ILogger<DiscordService> _logger;
    private readonly KimiConfiguration _configuration;

    private readonly string _token;

    private readonly DiscordSocketClient _client;
    private readonly CommandHandler _command;
    private readonly InteractionHandler _interaction;

    private readonly RankingService _rankingService;
    private readonly BirthdayService _birthdayService;

    private readonly GuildRepository _guildRepository;
    private readonly ProfileRepository _profileRepository;

    private readonly ulong[] _guilds;

    public DiscordService(ILogger<DiscordService> logger, DiscordSocketClient client,
        CommandHandler command, InteractionHandler interaction, IServiceScopeFactory scopeFactory,
        IConfiguration config, IOptions<KimiConfiguration> options, BirthdayService birthdayService)
    {
        _logger = logger;
        _configuration = options.Value;
        _token = config["Discord:Token"] ?? throw new Exception("Token is missing.");
        _client = client;
        _command = command;
        _interaction = interaction;

        var scope = scopeFactory.CreateScope();
        _guildRepository = scope.ServiceProvider.GetRequiredService<GuildRepository>();
        _profileRepository = scope.ServiceProvider.GetRequiredService<ProfileRepository>();

        _rankingService = scope.ServiceProvider.GetRequiredService<RankingService>();
        _birthdayService = birthdayService;

        _guilds = options.Value.Guilds.Select(x => x.Id).ToArray();
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
        await _birthdayService.InitializeAsync();

        var profile = await _profileRepository.GetAsync();
        if (profile is not null)
        {
            await _client.SetStatusAsync(profile.StatusType);
            await _client.SetGameAsync(profile.StatusMessage, profile.StatusUrl, profile.StatusActivityType);
        }

        await _interaction.RegisterModulesAsync();

        _client.Ready -= OnReadyAsync;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await _client.StopAsync();
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