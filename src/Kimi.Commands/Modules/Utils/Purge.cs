using System.Diagnostics;
using Discord;
using Discord.Commands;
using Kimi.Logging;

namespace Kimi.Commands.Modules.Utils
{
    public class Purge : ModuleBase<SocketCommandContext>
    {
        [RequireOwner]
        [Command("purge")]
        public async Task PurgeAsync(string path)
        {
            IMessageChannel message = Context.Channel;

            var file = (await File.ReadAllLinesAsync(path)).ToList();

            await Log.Write("Found!");

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            foreach (var line in file)
            {
                var item = await message.GetMessageAsync(ulong.Parse(line));
                await Log.Write($"Got {item.Author} ({item.Id}) - {item.Content} <{item.Timestamp}>");
                await item.DeleteAsync();
                await Log.Write("Deleted!");
                await Task.Delay(1000);
            }

            stopwatch.Stop();
            await Log.Write($"Elapsed - {stopwatch.Elapsed.Hours:00}:{stopwatch.Elapsed.Minutes:00}:{stopwatch.Elapsed.Seconds:00}");

            await ReplyAsync(
                $"Elapsed - {stopwatch.Elapsed.Hours:00}:{stopwatch.Elapsed.Minutes:00}:{stopwatch.Elapsed.Seconds:00}");
        }
    }
}
