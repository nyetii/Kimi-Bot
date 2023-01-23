using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Kimi.Core.Services;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Kimi.Core.Services.Interfaces;
using Kimi.GPT2;

namespace Kimi.Core.Modules.Monark
{
    public partial class Monark
    {
        private static readonly Random Rng = new Random();
        public static async Task HandleSubCommands(SocketSlashCommand command)
        {
            _ = command.Data.Options.First().Name switch
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
            try
            {
                await command.DeferAsync();

                Cache cache = new();
                ICommandQuery context = new ContextCommandData(command);
                
                var legacyMode = await context.GetValue("legacy-mode");
                //var legacyMode = command.Data.Options.First(x => x.Name == "generate").Options.FirstOrDefault().Value;
                legacyMode ??= false;

                if (legacyMode is null or false)
                {
                    var generation = await cache.GetFromCache();
                    await command.ModifyOriginalResponseAsync(async m =>
                    {
                        m.Content = $"https://twitter.com/monark/status/{command.Id}/";
                        m.Embed = await TweetEmbed(generation);
                    });
                }
                else
                    await command.FollowupAsync(text: $"https://twitter.com/monark" +
                                                      //$"/status/{Rng.NextInt64(1000000000000000000, Int64.MaxValue)}/",
                                                      $"/status/{command.Id}/",
                        embed: await TweetEmbed(await TweetData.GetTweet()));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        private static async Task MonarkForce(SocketSlashCommand command)
        {
            try
            {
                ICommandQuery context = new ContextCommandData(command);

                var tweet = await context.GetValue("tweet");
                var image = (Attachment?)await context.GetValue("image");
                var avatar = (Attachment?)await context.GetValue("avatar");
                var username = (string?)await context.GetValue("username");
                var nickname = (string?)await context.GetValue("nickname");

                await command.RespondAsync(text: $"https://twitter.com/{(username != null ? WhiteSpacesRegex().Replace(username, "").ToLowerInvariant() : "monark")}" +
                                                 $"/status/{Rng.NextInt64(1000000000000000000, Int64.MaxValue)}/",
                    embed: await TweetEmbed(tweet, image, avatar, username, nickname));
            }
            catch (Exception ex) { await command.RespondAsync(ex.ToString()); }
        }

        private static async Task<Embed> TweetEmbed(object tweet, Attachment? image = null, Attachment? avatar = null, object? username = null, object? nickname = null)
        {


            var author = new EmbedAuthorBuilder()
                .WithIconUrl(avatar != null ? $"{avatar.Url}" : "https://pbs.twimg.com/profile_images/1414588664169041920/zOl8EzRT_400x400.jpg")
                .WithName(username != null ? $"{nickname ?? username} " +
                                             $"(@{WhiteSpacesRegex().Replace((string)username, "").ToLowerInvariant()})" :
                    nickname != null ? $"{nickname} (@monark)" : "♔ Monark (@monark)");
            var footer = new EmbedFooterBuilder()
                .WithIconUrl("https://abs.twimg.com/icons/apple-touch-icon-192x192.png")
                .WithText("Twitter");
            var likes = new EmbedFieldBuilder()
                .WithName("Likes")
                .WithValue(KimiData.TweetData.Select(x => x.public_metrics.like_count.ToString()).ElementAt(Rng.Next(0, KimiData.TweetData.Count)))
                .WithIsInline(true);
            var retweets = new EmbedFieldBuilder()
                .WithName("Retweets")
                .WithValue(KimiData.TweetData.Select(x => x.public_metrics.retweet_count.ToString()).ElementAt(Rng.Next(0, KimiData.TweetData.Count)))
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

        [GeneratedRegex("\\s+")]
        private static partial Regex WhiteSpacesRegex();
    }
}
