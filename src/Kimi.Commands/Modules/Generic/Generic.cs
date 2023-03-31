using Discord;
using Discord.Commands;
using Kimi.Services.Core;

namespace Kimi.Commands.Modules.Generic
{
    public class Generic : ModuleBase<SocketCommandContext>
    {
        [Command("polyana")]
        [Summary("Primeiro comando feito para o bot")]
        public async Task HandlePolyanaCommand()
        {
            await Context.Message.ReplyAsync("https://media.discordapp.net/attachments/896243750228090974/952247379732619326/lobber.gif");
        }

        [Command("monoana")]
        [Summary("Primeiro comando feito para o bot")]
        public async Task HandleMonoanaCommand()
        {
            await Context.Message.ReplyAsync("https://media.discordapp.net/attachments/896243750228090974/952247379732619326/lobber.gif");
        }

        [Command("info")]
        public async Task HandleHelpCommand()
        {
            var embed = new EmbedBuilder()
                .WithAuthor("Hello there, I'm Kimi!",
                    Context.Client.CurrentUser.GetAvatarUrl())
                .WithDescription("Sorry, I am still yet to get better!")
                .WithFooter("Version " + Info.Version)
                .WithColor(241, 195, 199)
                .Build();

            await ReplyAsync(embed: embed);
        }

        [Command("ping")]
        public async Task HandleTruePingCommand()
        {
            await ReplyAsync($"{Context.Client.Latency}ms <:pacemike:841313592179163166>");
        }

        [Command("say")]
        public async Task HandleSayCommand([Remainder] string message) => await ReplyAsync(message);
    }
}
