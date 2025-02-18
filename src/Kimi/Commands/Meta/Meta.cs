using System.Text;
using Discord.Commands;
using InteractionService = Discord.Interactions.InteractionService;

namespace Kimi.Commands.Meta;

[RequireOwner]
[Group("meta")]
public class Meta : ModuleBase<SocketCommandContext>
{
    private readonly InteractionService _interaction;

    public Meta(InteractionService interaction)
    {
        _interaction = interaction;
    }

    [Command("refresh")]
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

        await ReplyAsync($"These are the currently active commands on this server:\n{commandsStr}");
    }
}