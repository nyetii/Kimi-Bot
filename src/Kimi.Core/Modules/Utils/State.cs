using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;
using Discord.Commands;

namespace Kimi.Core.Modules.Utils
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
