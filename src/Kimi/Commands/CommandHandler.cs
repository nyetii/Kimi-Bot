using System.Reflection;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace Kimi.Commands;

public class CommandHandler
{
    private readonly string[] _prefix;

    private readonly ILogger<CommandHandler> _logger;

    private readonly DiscordSocketClient _client;
    private readonly CommandService _commandService;
    private readonly IServiceProvider _serviceProvider;

    public CommandHandler(ILogger<CommandHandler> logger, DiscordSocketClient client, CommandService commandService, IConfiguration config, IServiceProvider serviceProvider)
    {
        _prefix = config.GetSection("Discord:Prefix").Get<string[]>() ?? [];

        _logger = logger;

        _client = client;
        _commandService = commandService;
        _serviceProvider = serviceProvider;
    }

    public async Task InitializeAsync()
    {
        await _commandService.AddModulesAsync(Assembly.GetExecutingAssembly(), _serviceProvider);

        _client.MessageReceived += OnMessageReceived;
        _commandService.CommandExecuted += OnCommandExecuted;
    }

    private async Task OnMessageReceived(SocketMessage socketMessage)
    {
        if (!HasPrefix(socketMessage, out var argPos))
            return;

        var context = new SocketCommandContext(_client, socketMessage as SocketUserMessage);

        await _commandService.ExecuteAsync(context, argPos, _serviceProvider);
    }

    private async Task OnCommandExecuted(Optional<CommandInfo> command, ICommandContext context, IResult result)
    {
        if (!result.IsSuccess)
        {
            var message = result.Error switch
            {
                CommandError.UnknownCommand => "Unknown command.",
                CommandError.ParseFailed => $"Parse failed: {result.ErrorReason}",
                CommandError.BadArgCount => "Invalid arguments.",
                CommandError.ObjectNotFound => "Arguments could not be interpreted.",
                CommandError.MultipleMatches => "There are multiple matches.",
                CommandError.UnmetPrecondition => $"Unmet precondition: {result.ErrorReason}",
                CommandError.Exception => $"Exception: {result.ErrorReason}",
                _ => "Command could not be executed."
            };

            if (result.Error is CommandError.UnknownCommand)
                return;


            _logger.Log(result.Error is CommandError.Exception ? LogLevel.Error : LogLevel.Warning,
                "Command {module} > {command}: {message}",
                command.Value.Module.Group ?? command.Value.Module.Name, command.Value.Name, message);

            await context.Message.ReplyAsync(message);
            return;
        }

        _logger.LogDebug("Command {module} > {command}: Executed successfully by {user}.",
            command.Value.Module.Group ?? command.Value.Module.Name, command.Value.Name, context.User.Username);
    }

    private bool HasPrefix(IMessage message, out int argPos)
    {
        argPos = 0;

        if (message is not SocketUserMessage socketMessage || message.Author.IsBot)
            return false;

        var pos = argPos;

        var hasPrefix = _prefix.Any(prefix => socketMessage.HasStringPrefix(prefix, ref pos));

        if (!hasPrefix && !socketMessage.HasMentionPrefix(_client.CurrentUser, ref pos))
            return false;

        argPos = pos;

        return true;
    }
}