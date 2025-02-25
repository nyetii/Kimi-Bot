using System.Reflection;
using System.Text;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Kimi.Configuration;
using Kimi.Extensions;
using Microsoft.Extensions.Options;
using IResult = Discord.Interactions.IResult;

namespace Kimi.Commands;

public class InteractionHandler
{
    private readonly ILogger<InteractionHandler> _logger;
    private readonly KimiConfiguration _configuration;

    private readonly DiscordSocketClient _client;
    private readonly InteractionService _interactionService;
    private readonly IServiceProvider _serviceProvider;

    public InteractionHandler(ILogger<InteractionHandler> logger, IOptions<KimiConfiguration> options,
        DiscordSocketClient client, InteractionService interactionService, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _configuration = options.Value;
        _client = client;
        _interactionService = interactionService;
        _serviceProvider = serviceProvider;
    }

    public async Task InitializeAsync()
    {
        await _interactionService.AddModulesAsync(Assembly.GetExecutingAssembly(), _serviceProvider);

        _client.InteractionCreated += OnInteractionCreated;
        _interactionService.InteractionExecuted += OnInteractionExecuted;
    }

    public async Task RegisterModulesAsync()
    {
        var commands = await _interactionService.RegisterCommandsGloballyAsync();

        _logger.LogInformation("There are {count} global modules: {commands}", commands.Count,
            string.Join(", ", commands.Select(x => x.Name)));
    }

    private async Task OnInteractionCreated(SocketInteraction interaction)
    {
        _logger.LogInformation("Interaction created");
        var context = new SocketInteractionContext(_client, interaction);
        await _interactionService.ExecuteCommandAsync(context, _serviceProvider);
    }

    private async Task OnInteractionExecuted(ICommandInfo command, IInteractionContext context, IResult result)
    {
        if (!result.IsSuccess)
        {
            var message = result.Error switch
            {
                InteractionCommandError.UnknownCommand => "Unknown command.",
                InteractionCommandError.ConvertFailed => $"Convert failed: {result.ErrorReason}",
                InteractionCommandError.BadArgs => "Invalid arguments.",
                InteractionCommandError.Exception => $"Exception: {result.ErrorReason}",
                InteractionCommandError.Unsuccessful => "Unsuccessful interaction.",
                InteractionCommandError.UnmetPrecondition => $"Unmet precondition: {result.ErrorReason}",
                InteractionCommandError.ParseFailed => $"Parse failed: {result.ErrorReason}",
                _ => "Command could not be executed."
            };

            _logger.Log(result.Error is InteractionCommandError.Exception ? LogLevel.Error : LogLevel.Warning,
                "Command {command}: {message}",
                command.Name, message);

            if (context.Interaction.HasResponded)
                await context.Interaction.FollowupAsync(
                    $"Something went wrong, {context.Client.GetApplicationInfoAsync().Result.Owner.Username} has been notified.");
            else
                await context.Interaction.RespondAsync(
                    $"Something went wrong, {context.Client.GetApplicationInfoAsync().Result.Owner.Username} has been notified.");

            var strBuilder
                = new StringBuilder(
                    $"Something went wrong with the {command.Module.SlashGroupName} {command.Name} command on <#{context.Channel.Id}>\n");

            strBuilder.AppendLine("```yaml");
            strBuilder.AppendLine($"Command: /{command.Module.SlashGroupName} {command.Name}");
            strBuilder.AppendLine(
                $"Method: {command.MethodName}({
                    string.Join(", ", command.Parameters
                        .Select(p => $"{p.ParameterType.Name} {p.Name}")
                    )})");
            strBuilder.AppendLine(message);
            strBuilder.AppendLine("");

            if (context.Interaction.Data is SocketSlashCommandData data)
            {
                if (data.Options.FirstOrDefault(x => x.Name == command.Name)?.Options.Count > 0)
                    strBuilder.AppendLine("!PARAMETERS");
                foreach (var option in data.Options.SelectMany(x => x.Options))
                {
                    strBuilder.AppendLine($"- {option.Type} {option.Name}: {option.Value}");
                }
            }

            strBuilder.AppendLine("```");

            await _client.SendToLogChannelAsync(_configuration.LogChannel, strBuilder.ToString());

            return;
        }

        _logger.LogDebug("Command {command}: Executed successfully by {user}.",
            command.Name, context.User.Username);
    }
}