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

        //[RequireOwner]
        //[Command("purge")]
        //public async Task PurgeAsync(ulong? id = null, ISocketMessageChannel? channel = null)
        //{
        //    if (!id.Equals(null))
        //    {
        //        var stopwatch = new Stopwatch();
        //        var foundAuthor = false;
        //        var foundMessages = 0;

        //        try
        //        {
        //            stopwatch.Start();

        //            var messages = await Context.Channel
        //                .GetMessagesAsync(limit: int.MaxValue, CacheMode.AllowDownload, RequestOptions.Default)
        //                .FlattenAsync();

        //            stopwatch.Stop();
        //            await Logging.LogAsync("All messages downloaded!");
        //            await Logging.LogAsync($"Elapsed - {stopwatch.Elapsed.Hours:00}:{stopwatch.Elapsed.Minutes:00}:{stopwatch.Elapsed.Seconds:00}");
        //            stopwatch.Reset();
        //            stopwatch.Start();

        //            foreach (var message in messages)
        //            {
        //                if (!message.Author.Id.Equals(id)) continue;
        //                foundAuthor = true;
        //                await Logging.LogAsync($"Found message\n{message.CleanContent}");
        //                foundMessages++;
        //                await message.DeleteAsync();
        //                await Task.Delay(1000);
        //            }
        //            stopwatch.Stop();
        //            await Logging.LogAsync($"Elapsed - {stopwatch.Elapsed.Hours:00}:{stopwatch.Elapsed.Minutes:00}:{stopwatch.Elapsed.Seconds:00}");
        //            await ReplyAsync(foundAuthor
        //                ? $"Done! Found {foundMessages} messages in this channel, check if they were properly deleted"
        //                : "Author not found, did you type in the correct id?");
        //        }
        //        catch (Exception ex)
        //        {
        //            await ReplyAsync(ex.Message);
        //        }

        //        return;
        //    }

        //    await ReplyAsync("Invalid id");

        //}
    }
}
