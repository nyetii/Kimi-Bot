using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Net;
using Discord.WebSocket;
using Kimi.Core.Services;
using Kimi.Core.Services.Interfaces;

namespace Kimi.Core.Modules.Monark
{
    internal class MonarkInitialize : ICommandInitializer
    {
        private readonly DiscordSocketClient _client;

        public MonarkInitialize(DiscordSocketClient client)
        {
            _client = client;
        }

        public async Task Initialize()
        {
            await DictionaryEntries();

            //var z = Info.CommandInfo.First(x => x.Key.Equals("monark")).Value;
            //var z = Info.CommandInfo.SelectMany(x => x.Value.GetType));
            //E(z);
            //var abc = Info.CommandInfo["monark"];
            //foreach (KeyValuePair<string,dynamic> item in abc)
            //{
            //    var a1 = (string)item.Value;
            //}

            ICommandQuery cq = new CategoryQuery();
            string[] category = await cq.GetKeys();

            ICommandQuery sq = new SubcategoryQuery();
            string[] subcategory = await sq.GetKeys();

            ICommandQuery pq = new ParameterQuery();
            string[] parameter = await pq.GetKeys();

            //SubcategoryQuery cq = new();
            //string[] absol = await cq.GetKeys();

            //ParameterQuery mb = new();
            //string[] jose = await mb.GetKeys();
            

            var monarkCommands = new SlashCommandBuilder()
            .WithName(category[0])
            .WithDescription(await sq.GetValue(subcategory[0]))
            .AddOption(new SlashCommandOptionBuilder()
                .WithName(subcategory[1])
                .WithDescription(await sq.GetValue(subcategory[1]))
                .WithType(ApplicationCommandOptionType.SubCommand))
            .AddOption(new SlashCommandOptionBuilder()
                .WithName(subcategory[2])
                .WithDescription(await pq.GetValue(parameter[0]))
                .WithType(ApplicationCommandOptionType.SubCommand)
                .AddOption(new SlashCommandOptionBuilder()
                    .WithName(parameter[1])
                    .WithDescription(await pq.GetValue(parameter[1]))
                    .WithType(ApplicationCommandOptionType.String)
                    .WithMaxLength(280)
                    .WithMinLength(1)
                    .WithRequired(true))
                .AddOption(new SlashCommandOptionBuilder()
                    .WithName(parameter[2])
                    .WithDescription(await pq.GetValue(parameter[2]))
                    .WithType(ApplicationCommandOptionType.Attachment))
                .AddOption(new SlashCommandOptionBuilder()
                    .WithName(parameter[3])
                    .WithDescription(await pq.GetValue(parameter[3]))
                    .WithType(ApplicationCommandOptionType.Attachment))
                .AddOption(new SlashCommandOptionBuilder()
                    .WithName(parameter[4])
                    .WithDescription(await pq.GetValue(parameter[4]))
                    .WithType(ApplicationCommandOptionType.String)
                    .WithMaxLength(15)
                    .WithMinLength(3))
                .AddOption(new SlashCommandOptionBuilder()
                    .WithName(parameter[5])
                    .WithDescription(await pq.GetValue(parameter[5]))
                    .WithType(ApplicationCommandOptionType.String)
                    .WithMaxLength(50)
                    .WithMinLength(4)))
            .AddOption(new SlashCommandOptionBuilder()
                .WithName(subcategory[3])
                .WithDescription(await sq.GetValue(subcategory[3]))
                .WithType(ApplicationCommandOptionType.SubCommand));

            await Task.CompletedTask;
            try
            {
                await _client.Rest.CreateGuildCommand(monarkCommands.Build(), 973401092274659358);
            }
            catch(HttpException ex) { }
        }
        

        private static async Task DictionaryEntries()
        {
            Info.CommandInfo.Add(
                "monark",
                new Dictionary<string, dynamic>()
                {
                    { "description", "Gerar um tweet aleatório do Monark" },
                    { "generate", "Gerar um tweet aleatório do Monark"},
                    { "force", new Dictionary<string, string>()
                    {
                        { "description", "Gerar um tweet"},
                        { "tweet", "Ex.: Olá Mundo!" },
                        { "imagem", "Envie uma iamgem"},
                        { "avatar", "Envie uma imagem"},
                        { "username", "Nome de usuário"},
                        { "nickname", "Apelido"}
                    }},
                    { "count", "Conta a quantidade de tweets do Monark"}
                }
                );

            await Task.CompletedTask;
        }
    }
}
