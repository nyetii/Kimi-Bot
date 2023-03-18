using Discord.Commands;

namespace Kimi.Core._Modules.Utils
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
