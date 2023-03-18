using System.Reflection;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Kimi.Logging;

namespace Kimi.Core._Modules.Utils
{
    public class RefreshSlashCommand : ModuleBase<SocketCommandContext>
    {
        private DiscordSocketClient? Client { get; set; }

        [RequireOwner]
        [Command("refreshSlashCommand")]
        [Alias("refresh")]
        public async Task HandleRefreshSlashCommand(string command)
        {
            try
            {
                await Log.Write("a");
                bool refreshRequested = true;
                Client = Context.Client;

                command = $"Kimi.Core.Modules.{command}.{command}Initialize";

                Type? t = Type.GetType(command);

                if (t is null)
                {
                    await Context.Message.ReplyAsync("Type not found");
                    return;
                }

                object? instance = Activator.CreateInstance(t, args: new dynamic[] { Client, refreshRequested });

                MethodInfo? initialize = t.GetMethod("Initialize", BindingFlags.Public | BindingFlags.Instance);
                //MethodInfo? createCommand = t.GetMethod("CreateCommand", BindingFlags.Public | BindingFlags.Instance);

                if (initialize is null)
                {
                    await ReplyAsync("Method not found");
                    return;
                }

                initialize.Invoke(instance, null);
                //createCommand.Invoke(instance, null);

                await Log.Write($"{t.Name} updated!");
                await ReplyAsync($"{t.Name} updated!");
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
