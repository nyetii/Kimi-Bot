using Discord;
using Discord.Commands;

namespace Kimi.Core._Modules
{
    public class PrefixModule : ModuleBase<SocketCommandContext>
    {
        [Command("polyana")]
        [Summary("Primeiro comando feito para o bot")]
        public async Task HandlePingCommand()
        {
            await Context.Message.ReplyAsync("https://media.discordapp.net/attachments/896243750228090974/952247379732619326/lobber.gif");
        }
    }
}
