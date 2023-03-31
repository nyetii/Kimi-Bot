using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;
using Kimi.Services.Commands;

namespace Kimi.Commands.Modules.Utils
{
    public class Eval : ModuleBase<SocketCommandContext>
    {
        private readonly DiscordSocketClient _client;

        public Eval(DiscordSocketClient client)
        {
            _client = client;
        }

        [RequireOwner]
        [Command("eval")]
        public async Task HandleEvalCommand([Remainder] string code)
        {
            await new EvalHandler(_client, Context).EvalAsync(code);
        }
    }
}
