using System.Text;
using Discord.Commands;
using InteractionService = Discord.Interactions.InteractionService;

namespace Kimi.Commands;

[RequireOwner]
[Group("meta")]
public class Meta : ModuleBase<SocketCommandContext>
{
    private readonly InteractionService _interaction;

    public Meta(InteractionService interaction)
    {
        _interaction = interaction;
    }

    [Command("register")]
    public async Task RefreshSlashCommands()
    {
        var commands = await _interaction.RegisterCommandsToGuildAsync(Context.Guild.Id);

        var commandsStr = new StringBuilder();
        foreach (var command in commands)
        {
            if (string.IsNullOrWhiteSpace(command.Description))
                commandsStr.AppendLine($"1. **{command.Name}**");
            else
                commandsStr.AppendLine($"1. **{command.Name}** - {command.Description}");
        }

        await ReplyAsync(commands.Count is 0
            ? "No new commands were added."
            : $"Added the following commands to the server:\n" +
              $"{commandsStr}");
    }
}