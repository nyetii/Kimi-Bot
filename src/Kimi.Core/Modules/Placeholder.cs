using Discord.Interactions;
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
    }
}
