using Discord;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Kimi.Modules.Monark
{
    class MonarkSerialization
    {
        public static List<Datum> TweetData = new List<Datum>();
        static Root monark;

        public static async Task<string> DeserializationAsync()
        {
            try
            {
                string jsonString = File.ReadAllText($"{Environment.CurrentDirectory}/Modules/Monark/tweets.json");

                monark = JsonConvert.DeserializeObject<Root>(jsonString)!;
                foreach(var bulk in monark.bulk)
                {
                    foreach(var data in bulk.data)
                    {
                        TweetData.Add(data);
                    }
                    
                }
                
                return await Task.FromResult("The .json file has been deserialized! 👍");
            }
            catch(Exception ex) 
            { 
                return await Task.FromResult($"```csharp\n{ex}\n```"); 
            }
        }

        public static async Task<string> TweetCount()
        {
            try
            {
                int tweets = 0;
                foreach (var bulk in monark.bulk)
                {
                    foreach (var tweet in bulk.data)
                    {
                        tweets++;
                    }

                }

                return await Task.FromResult($"Monark wrote {tweets} tweets so far.");
            }
            catch (Exception ex)
            {
                return await Task.FromResult($"```csharp\n{ex}\n```");
            }
        }

        public static async Task<string> GenerateAsync()
        {
            try
            {
                Random rng = new Random();

                string[] p1;
                string[] p2;

                int n = rng.Next(0, TweetData.Count);

                // p1 = TweetData[n].text.ToString().Split(',', '.');
                p1 = Regex.Replace(TweetData[n].text, @"http[^\s]+", "").Split(',', '.');
                n = rng.Next(TweetData.Count);
                p2 = Regex.Replace(TweetData[n].text, @"http[^\s]+", "").Split(',', '.');
                return await Task.FromResult($"{p1[rng.Next(0, p1.Length)].TrimEnd(' ')} {p2[rng.Next(0, p2.Length)].TrimEnd(' ')}");

            }
            catch(Exception ex)
            {
                return await Task.FromResult($"```csharp\n{ex}\n```");
            }
        }

        public static async Task<Embed> EmbedBuildAsync(MonarkArgs args)
        {
            var user = args.Username != null ? args.Username : null;
            if (args.Username != null) { user = Regex.Replace(args.Username, @"\s+", ""); }
            Random rng = new Random();

            var author = new EmbedAuthorBuilder()
                .WithIconUrl(args.Avatar != null ? $"{args.Avatar}" : "https://pbs.twimg.com/profile_images/1414588664169041920/zOl8EzRT_400x400.jpg")
                .WithName(args.Username != null ? $"{args.Username} (@{user.ToLowerInvariant()})" : "♔ Monark (@monark)");
            var footer = new EmbedFooterBuilder()
                .WithIconUrl("https://abs.twimg.com/icons/apple-touch-icon-192x192.png")
                .WithText("Twitter");
            var likes = new EmbedFieldBuilder()
                .WithName("Likes")
                .WithValue(TweetData[rng.Next(0, TweetData.Count)].public_metrics.like_count.ToString())
                .WithIsInline(true);
            var retweets = new EmbedFieldBuilder()
                .WithName("Retweets")
                .WithValue(TweetData[rng.Next(0, TweetData.Count)].public_metrics.retweet_count.ToString())
                .WithIsInline(true);

            var embed = new EmbedBuilder()
                .WithAuthor(author)
                .WithFooter(footer)
                .WithDescription(args.Tweet)
                .WithImageUrl(args.Image != null ? $"{args.Image}" : "")
                .AddField(likes)
                .AddField(retweets)
                .WithColor(29, 160, 242)
                .WithCurrentTimestamp()
                .Build();

            return await Task.FromResult(embed);
        }
    }
}
