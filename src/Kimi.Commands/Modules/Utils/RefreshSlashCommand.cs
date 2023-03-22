using System.Reflection;
using System.Runtime.CompilerServices;
using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using Kimi.Logging;
using Kimi.Services.Core;

namespace Kimi.Commands.Modules.Utils
{
    public class RefreshSlashCommand : ModuleBase<SocketCommandContext>
    {
        private InteractionService _slash;
        private readonly ulong[]? _guildId;

        public RefreshSlashCommand(InteractionService slash, Settings settings)
        {
            _slash = slash;
            _guildId = settings.General.DebugGuildId;
        }

        [Discord.Commands.RequireOwner]
        [Command("refreshSlashCommand")]
        [Alias("refresh")]
        public async Task HandleRefreshSlashCommand()
        {
            try
            {
                if (Info.IsDebug && _guildId != null)
                    foreach (var guild in _guildId)
                        await _slash.RegisterCommandsToGuildAsync(guild, true);
                else
                    await _slash.RegisterCommandsGloballyAsync();
                await Log.Write($"updated!");
                await ReplyAsync($"updated!");
            }
            catch (Exception ex)
            {
                await ReplyAsync(ex.ToString());
            }
        }

        [Command("test")]
        public async Task Test()
        {
            await ReplyAsync("it works?");
        }
    }
}
