using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.WebSocket;

namespace Kimi.Commands
{
    public class ComponentHandler
    {
        private readonly DiscordSocketClient _client;

        public ComponentHandler(DiscordSocketClient client)
        {
            _client = client;

            _client.ButtonExecuted += OnButtonExecuted;
        }

        private async Task OnButtonExecuted(SocketMessageComponent component)
        {

        }
    }
}
