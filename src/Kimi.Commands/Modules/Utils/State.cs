using Discord.Commands;
using Discord.Interactions;
using Kimi.Logging;
using Serilog;
using RequireOwner = Discord.Commands.RequireOwnerAttribute;

namespace Kimi.Commands.Modules.Utils
{
    public class State : ModuleBase<SocketCommandContext>
    {
        [RequireOwner]
        [Command("shutdown")]
        public async Task Shutdown()
        {
            await ReplyAsync("Bye!");
            Environment.Exit(0);
        }

    }
}
