using System.Text.RegularExpressions;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Kimi.GPT2;
using Kimi.Logging;
using Kimi.Services.Commands;
using Kimi.Services.Commands.Interfaces;
using Kimi.Services.Core;

namespace Kimi.Commands.Modules.Monark
{
    [Group("monark", "Gerar um tweet aleatório do Monark")]
    public partial class Monark : InteractionModuleBase<SocketInteractionContext>
    {
        private static readonly Random Rng = new Random();

        [SlashCommand("count", "Conta a quantidade de tweets do Monark")]
        public async Task HandleMonarkCountCommand() => await RespondAsync(TweetData.TData.Select(x => x?.text).Count().ToString());

        [SlashCommand("generate", "Gerar um tweet aleatório do Monark")]
        public async Task MonarkGenerate(bool legacyMode = false)
        {
            try
            {
                await DeferAsync();
                bool isEnabled = await Model.IsReady();

                Cache cache = new();

                if (Cache.Count == 0)
                    await FollowupAsync(text: await cache.NewCache());
                if (legacyMode is false && isEnabled)
                {
                    var generation = await cache.GetFromCache();
                    await ModifyOriginalResponseAsync(async m =>
                    {
                        m.Content = $"https://twitter.com/monark/status/{Context.Interaction.Id}/";
                        m.Embed = await TweetEmbed(generation);
                    });
                }
                else
                    await FollowupAsync(text: $"https://twitter.com/monark" +
                                                      $"/status/{Context.Interaction.Id}/",
                        embed: await TweetEmbed(await TweetData.GetTweet()));
            }
            catch (Exception ex)
            {
                await Log.Write(ex.ToString(), Severity.Error);
            }
        }

        [SlashCommand("force", "Gerar um tweet")]
        public async Task MonarkForce(string tweet, Attachment? image = null, Attachment? avatar = null, string? username = null, string? nickname = null)
        {
            try
            {
                await RespondAsync(text: $"https://twitter.com/{(username != null ? WhiteSpacesRegex().Replace(username, "").ToLowerInvariant() : "monark")}" +
                                         $"/status/{Context.Interaction.Id}/",
                    embed: await TweetEmbed(tweet, image, avatar, username, nickname));
            }
            catch (Exception ex) { await RespondAsync(ex.ToString()); }
        }

        private static async Task<Embed> TweetEmbed(object tweet, IAttachment? image = null, IAttachment? avatar = null, object? username = null, object? nickname = null)
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
                .WithValue(TweetData.TData.Select(x => (string)x.public_metrics.like_count.ToString()).ElementAt(Rng.Next(0, TweetData.TData.Count)))
                .WithIsInline(true);
            var retweets = new EmbedFieldBuilder()
                .WithName("Retweets")
                .WithValue(TweetData.TData.Select(x => (string)x.public_metrics.retweet_count.ToString()).ElementAt(Rng.Next(0, TweetData.TData.Count)))
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
