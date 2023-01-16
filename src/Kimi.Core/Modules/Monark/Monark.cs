using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Kimi.Core.Services;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Kimi.Core.Modules.Monark
{
    public class Monark : InteractionModuleBase<SocketInteractionContext>
    {
        public static async Task HandleCommandGroup(SocketSlashCommand command)
        {
            var subcommands = command.Data.Options.First().Name switch
            {
                "count" => MonarkCount(command),
                "generate" => MonarkGenerate(command),
                "force" => MonarkForce(command),
                _ => MonarkGenerate(command)
            };

            await Task.CompletedTask;
        }

        private static async Task MonarkCount(SocketSlashCommand command) => await command.RespondAsync(KimiData.TweetData.Select(x => x.text).Count().ToString());

        private static async Task MonarkGenerate(SocketSlashCommand command)
        {
            //await command.RespondAsync(await TweetData.GetTweet());
            await command.DeferAsync();
            await command.ModifyOriginalResponseAsync(async x => x.Content = await TweetData.GetTweet());
        }

        private static async Task MonarkForce(SocketSlashCommand command)
        {
            try
            {
                var fieldName = command.Data.Options.First().Options.First(x => x.Name == "avatar").Name;
                //var getOrSet = command.Data.Options.First().Options.First().Options.First().Name;
                // Since there is no value on a get command, we use the ? operator because "Options" can be null.
                //var jajaja = command.Data.Id;
                //object value = new object();
                //Attachment? bu = null;

                var tweet = command.Data.Options.First().Options.First(x => x.Name == "tweet").Value;                 // First "First()" = "force"; Second "First()" = "tweet";
                var image = (Attachment?)command.Data.Options.First().Options.First(x => x.Name == "image").Value;
                var avatar = (Attachment?)command.Data.Options.First().Options.First(x => x.Name == "avatar").Value;
                var username = command.Data.Options.First().Options.First(x => x.Name == "username").Value;
                var nickname = command.Data.Options.First().Options.First(x => x.Name == "nickname").Value;

                await command.RespondAsync(embed: await TweetEmbed(tweet, image, avatar, username, nickname));
                //await command.RespondAsync($"Tweet: {tweet}\nImage: {image.Url}\nAvatar: {avatar.Url}");
            } catch(Exception ex) { await command.RespondAsync(ex.ToString()); }
        }

        private static async Task<Embed> TweetEmbed(object tweet, Attachment? image = null, Attachment? avatar = null, object? username = null, object? nickname = null)
        {
            if (username != null)
                username = Regex.Replace((string)username, @"\s+", ""); 
            Random rng = new Random();

            var author = new EmbedAuthorBuilder()
                .WithIconUrl(avatar != null ? $"{avatar.Url}" : "https://pbs.twimg.com/profile_images/1414588664169041920/zOl8EzRT_400x400.jpg")
                .WithName(username != null ? $"{nickname ?? username} (@{username.ToString().ToLowerInvariant()})" : "♔ Monark (@monark)");
            var footer = new EmbedFooterBuilder()
                .WithIconUrl("https://abs.twimg.com/icons/apple-touch-icon-192x192.png")
                .WithText("Twitter");
            var likes = new EmbedFieldBuilder()
                .WithName("Likes")
                .WithValue(KimiData.TweetData.Select(x => x.public_metrics.like_count.ToString()).ElementAt(rng.Next(0, KimiData.TweetData.Count)))
                //.WithValue(TweetData[rng.Next(0, TweetData.Count)].public_metrics.like_count.ToString())
                .WithIsInline(true);
            var retweets = new EmbedFieldBuilder()
                .WithName("Retweets")
                .WithValue(KimiData.TweetData.Select(x => x.public_metrics.retweet_count.ToString()).ElementAt(rng.Next(0, KimiData.TweetData.Count)))
                //.WithValue(TweetData[rng.Next(0, TweetData.Count)].public_metrics.retweet_count.ToString())
                .WithIsInline(true);

            var embed = new EmbedBuilder()
                .WithAuthor(author)
                .WithFooter(footer)
                .WithDescription(tweet.ToString())
                .WithImageUrl(image != null ? $"{image.Url}" : "")
                .AddField(likes)
                .AddField(retweets)
                .WithColor(29, 160, 242)
                .WithCurrentTimestamp()
                .Build();

            return await Task.FromResult(embed);
        }
    }
}
