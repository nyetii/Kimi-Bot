using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kimi.Core.Modules.Generic
{
    public class Generic : ModuleBase<SocketCommandContext>
    {
        [Command("monoana")]
        [Summary("Primeiro comando feito para o bot")]
        public async Task HandlePingCommand()
        {
            await Context.Message.ReplyAsync("https://media.discordapp.net/attachments/896243750228090974/952247379732619326/lobber.gif");
        }
    }
}
