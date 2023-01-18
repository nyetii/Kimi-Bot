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
using Serilog;

namespace Kimi.Core.Modules.Monark
{
    internal class MonarkInitialize : ICommandInitializer
    {
        private bool _isRegistered;
        private readonly DiscordSocketClient _client;
        private SlashCommandBuilder _command = new();

        public MonarkInitialize(DiscordSocketClient client)
        {
            _client = client;
            _isRegistered = ICommandInitializer.IsRegistered;
        }

        public async Task Initialize()
        {
            await DictionaryEntries();

            ICommandQuery cq = new CategoryQuery();
            string[] category = await cq.GetKeys();

            ICommandQuery sq = new SubcategoryQuery();
            string[] subcategory = await sq.GetKeys();

            ICommandQuery pq = new ParameterQuery();
            string[] parameter = await pq.GetKeys();
            

            _command = new SlashCommandBuilder()
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
        }

        public async Task CreateCommand()
        {
            try
            {
                foreach (var item in _client.Rest.GetGlobalApplicationCommands().Result)
                {
                    if (!item.Name.Equals(_command.Name)) continue;
                    _isRegistered = true;
                    break;
                }
                if (!_isRegistered)
                {
                    if (Info.IsDebug)
                        await _client.Rest.CreateGuildCommand(_command.Build(), 973401092274659358);
                    else
                        await _client.Rest.CreateGlobalCommand(_command.Build());
                    await Logging.LogAsync($"{GetType().Name} - Command succesfully created!");
                    _isRegistered = true;
                }

            }
            catch (HttpException ex)
            {
                await Logging.LogAsync($"{GetType().Name} - Exception!\n{ex}", Severity.Error);
            }
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
