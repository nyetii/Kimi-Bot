using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Kimi.Modules.Monark
{
    [Group("monark")]

    class Monark : ModuleBase<SocketCommandContext>
    {


        [Command("force")]
        [Summary("Atributo para forçar um template personalizado. " +
            "Template padrão: @monark\n\n**Sintaxe:**\n`!monark force tweet: \"tweet\" image: <link> avatar: <link> username: \"username\"`")]
        public async Task Teste2([Summary("Lembre-se que os campos `tweet` e `username` precisam ser preenchidos com aspas - Ex.: \"Flow Podcast\"\n\n" +
            "`tweet`    - Conteúdo do tweet em questão\n" +
            "`image`    - URL de uma imagem\n" +
            "`avatar`   - URL de um avatar\n" +
            "`username` - Nome de usuário (espaços serão cortados para o @)")]
        [Remainder] MonarkArgs content)
        {
            var link = content.Username != null ? content.Username : "monark";
            if (content.Username != null) { link = Regex.Replace(content.Username, @"\s+", ""); }
            Random rng = new Random();
            await Context.Message.ReplyAsync(text: $"<https://twitter.com/{link.ToLowerInvariant()}/status/" +
                $"{MonarkSerialization.TweetData[rng.Next(0, MonarkSerialization.TweetData.Count)].id}>",
                embed: await MonarkSerialization.EmbedBuildAsync(content));

        }

        [Command("deserialize")]
        [Summary("Comando para desserializar o arquivo \"tweets.json\" do template de @monark")]
        public async Task Deserialize()
        {
            string output = await MonarkSerialization.DeserializationAsync();
            await Context.Message.ReplyAsync(output);
        }

        [Command("count")]
        [Summary("Conta a quantidade de tweets do monark")]
        public async Task TweetCount()
        {
            await Context.Message.ReplyAsync(await MonarkSerialization.TweetCount());
        }

        [Command(null)]
        [Summary("Gerar um tweet aleatório do Monark")]
        public async Task Teste1()
        {
            Random rng = new Random();
            MonarkArgs args = new();
            args.Tweet = await MonarkSerialization.GenerateAsync();
            await Context.Message.ReplyAsync(text: $"<https://twitter.com/monark/status/" +
                $"{MonarkSerialization.TweetData[rng.Next(0, MonarkSerialization.TweetData.Count)].id}>",
                embed: await MonarkSerialization.EmbedBuildAsync(args));
        }
    }

    [NamedArgumentType]
    public class MonarkArgs
    {
        public string Tweet { get; set; }
        public string Image { get; set; }
        public string Avatar { get; set; }
        public string Username { get; set; }
    }
}
