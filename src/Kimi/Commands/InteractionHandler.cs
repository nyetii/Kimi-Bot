using System.Reflection;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using IResult = Discord.Interactions.IResult;

namespace Kimi.Commands;

public class InteractionHandler
{
    private readonly ILogger<InteractionHandler> _logger;
    
    private readonly DiscordSocketClient _client;
    private readonly InteractionService _interactionService;
    private readonly IServiceProvider _serviceProvider;

    public InteractionHandler(ILogger<InteractionHandler> logger, DiscordSocketClient client, InteractionService interactionService, IServiceProvider serviceProvider)
    {
        _logger = logger;
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

            await context.Interaction.RespondAsync(message);
            return;
        }

        _logger.LogDebug("Command {command}: Executed successfully by {user}.",
            command.Name, context.User.Username);
    }
}
