using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;

namespace Kimi.Commands.Modules.Generic
{
    public class Help : InteractionModuleBase<SocketInteractionContext>
    {
        [SlashCommand("help", "Kimi comes in clutch!")]
        public async Task HandleHelpCommand()
        {
            await RespondAsync(embed: await HelpEmbed());
        }

        private static async Task<Embed> HelpEmbed()
        {
            var author = new EmbedAuthorBuilder()
                .WithName("Kimi comes in clutch!")
                .WithIconUrl("https://cdn.discordapp.com/emojis/783328274193448981.webp");

            var embed = new EmbedBuilder()
                .WithColor(0xf1c3c7)
                .WithAuthor(author)
                .WithDescription("For more specific information, type `/help <command>`\nFurther questions? Ping `netty#0725`")
                .AddField(info =>
                {
                    info.WithIsInline(true);
                    info.WithName("INFO");
                    info.WithValue("`help` *this embed*\n" +
                                   "`info` *info about the bot*\n" +
                                   "`ping` *gets the latency*");
                })
                .AddField(monark =>
                {
                    monark.WithIsInline(true);
                    monark.WithName("MONARK");
                    monark.WithValue("`generate`\n" +
                                "`force`\n" +
                                "`count`");
                })
                .Build();

            await Task.CompletedTask;
            return embed;
        }
    }
}
