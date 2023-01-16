using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Kimi.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kimi.Core.Modules
{
    public class Placeholder : InteractionModuleBase<SocketInteractionContext>
    {
        [SlashCommand("ping", "Receive a ping message!")]
        public async Task HandlePingCommand()
        {
            await RespondAsync("andre teixeira");
        }

        public static async Task HandleListRoleCommand(SocketSlashCommand command)
        {
            // We need to extract the user parameter from the command. since we only have one option and it's required, we can just use the first option.
            var guildUser = (SocketGuildUser)command.Data.Options.First().Value;

            // We remove the everyone role and select the mention of each role.
            var roleList = string.Join(",\n", guildUser.Roles.Where(x => !x.IsEveryone).Select(x => x.Mention));

            var embedBuiler = new EmbedBuilder()
                .WithAuthor(guildUser.ToString(), guildUser.GetAvatarUrl() ?? guildUser.GetDefaultAvatarUrl())
                .WithTitle("Roles")
                .WithDescription(roleList)
                .WithColor(Color.Green)
                .WithCurrentTimestamp();

            await Logging.LogAsync("HHHHHHHHHHHHHHHHHHHHHHHHH", Severity.Debug);

            // Now, Let's respond with the embed.
            await command.RespondAsync(embed: embedBuiler.Build());
        }
    }
}
