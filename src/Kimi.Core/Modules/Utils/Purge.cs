using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Services.Description;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Kimi.Core.Services;

namespace Kimi.Core.Modules.Utils
{
    public class Purge : ModuleBase<SocketCommandContext>
    {
        [RequireOwner]
        [Command("purge")]
        public async Task PurgeAsync(string path)
        {
            IMessageChannel message = Context.Channel;

            var file = (await File.ReadAllLinesAsync(path)).ToList();

            await Logging.LogAsync("Found!");

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            foreach (var line in file)
            {
                var item = await message.GetMessageAsync(ulong.Parse(line));
                await Logging.LogAsync($"Got {item.Author} ({item.Id}) - {item.Content} <{item.Timestamp}>");
                await item.DeleteAsync();
                await Logging.LogAsync("Deleted!");
                await Task.Delay(1000);
            }

            stopwatch.Stop();
            await Logging.LogAsync($"Elapsed - {stopwatch.Elapsed.Hours:00}:{stopwatch.Elapsed.Minutes:00}:{stopwatch.Elapsed.Seconds:00}");

            await ReplyAsync(
                $"Elapsed - {stopwatch.Elapsed.Hours:00}:{stopwatch.Elapsed.Minutes:00}:{stopwatch.Elapsed.Seconds:00}");
        }
    }
}
