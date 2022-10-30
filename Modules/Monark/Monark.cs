using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kimi.Modules.Monark
{
    [Group("monark")]
    class Monark : ModuleBase<SocketCommandContext>
    {
        [Command(null)]
        public async Task Teste1()
        {
            Random rng = new Random();
            string tweet = await MonarkSerialization.GenerateAsync();
            await Context.Message.ReplyAsync(text: $"<https://twitter.com/monark/status/" +
                $"{MonarkSerialization.TweetData[rng.Next(0, MonarkSerialization.TweetData.Count)].id}>", 
                embed: await MonarkSerialization.EmbedBuildAsync(tweet));
        }

        [Command("force")]
        public async Task Teste2([Remainder] string tweet)
        {
            Random rng = new Random();
            await Context.Message.ReplyAsync(text: $"<https://twitter.com/monark/status/" +
                $"{MonarkSerialization.TweetData[rng.Next(0, MonarkSerialization.TweetData.Count)].id}>",
                embed: await MonarkSerialization.EmbedBuildAsync(tweet));
        }

        [Command("deserialize")]
        public async Task Deserialize()
        {
            string output = await MonarkSerialization.DeserializationAsync();
            await Context.Message.ReplyAsync(output);
        }

        [Command("count")]
        public async Task TweetCount()
        {
            await Context.Message.ReplyAsync(await MonarkSerialization.TweetCount());
        }
    }
}
