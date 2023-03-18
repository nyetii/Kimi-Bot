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
using Kimi.GPT2;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Serilog;

namespace Kimi.Core.Modules.Monark
{
    internal class MonarkInitialize : ICommandInitializer
    {
        private bool _isEnabled = ICommandInitializer.IsEnabled;
        private bool _isRegistered;
        private readonly bool _refreshRequested;
        private readonly DiscordSocketClient _client;
        private SlashCommandBuilder _command = new();

        public MonarkInitialize(DiscordSocketClient client, bool refreshRequested = false)
        {
            _client = client;
            _isRegistered = ICommandInitializer.IsRegistered;
            _refreshRequested = refreshRequested;
        }

        public async Task Initialize()
        {
            _isEnabled = true;

            if(!_refreshRequested || !_isRegistered)
                await DictionaryEntries();

            ICommandQuery cq = new CategoryQuery();
            string[] category = await cq.GetKeys();

            ICommandQuery sq = new SubcategoryQuery();
            string[] subcategory = await sq.GetKeys();

            ICommandQuery pq = new ParameterQuery();
            string[] parameter = await pq.GetKeys();
            

            _command = new SlashCommandBuilder()
            .WithName("monark")
            .WithDescription("Gerar um tweet aleatório do Monark");

            _command.AddOption(new SlashCommandOptionBuilder()
                .WithName("generate")
                .WithDescription("Gerar um tweet aleatório do Monark")
                .WithType(ApplicationCommandOptionType.SubCommand)
                .AddOption(new SlashCommandOptionBuilder()
                    .WithName("legacy-mode")
                    .WithDescription("True = Geração antiga | False = Geração via GPT-2")
                    .WithType(ApplicationCommandOptionType.Boolean)));

            _command.AddOption(new SlashCommandOptionBuilder()
                .WithName("force")
                .WithDescription("Gerar um tweet")
                .WithType(ApplicationCommandOptionType.SubCommand)
                .AddOption(new SlashCommandOptionBuilder()
                    .WithName("tweet")
                    .WithDescription("Ex.: Olá Mundo!")
                    .WithType(ApplicationCommandOptionType.String)
                    .WithMaxLength(280)
                    .WithMinLength(1)
                    .WithRequired(true))
                .AddOption(new SlashCommandOptionBuilder()
                    .WithName("imagem")
                    .WithDescription("Envie uma iamgem")
                    .WithType(ApplicationCommandOptionType.Attachment))
                .AddOption(new SlashCommandOptionBuilder()
                    .WithName("avatar")
                    .WithDescription("Envie uma imagem")
                    .WithType(ApplicationCommandOptionType.Attachment))
                .AddOption(new SlashCommandOptionBuilder()
                    .WithName("username")
                    .WithDescription("Nome de usuário")
                    .WithType(ApplicationCommandOptionType.String)
                    .WithMaxLength(15)
                    .WithMinLength(3))
                .AddOption(new SlashCommandOptionBuilder()
                    .WithName("nickname")
                    .WithDescription("apelido")
                    .WithType(ApplicationCommandOptionType.String)
                    .WithMaxLength(50)
                    .WithMinLength(4)));

            _command.AddOption(new SlashCommandOptionBuilder()
                .WithName("count")
                .WithDescription("Conta a quantidade de tweets do Monark")
                .WithType(ApplicationCommandOptionType.SubCommand));

            if (_refreshRequested)
                await CreateCommand();
            await Task.CompletedTask;
        }

        public async Task CreateCommand()
        {
            try
            {
                if(!Info.IsDebug)
                    foreach (var item in _client.Rest.GetGlobalApplicationCommands().Result)
                    {
                        if (!item.Name.Equals(_command.Name)) continue;
                        _isRegistered = true;
                        break;
                    }
                if (!_isRegistered || _refreshRequested)
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
                    { "generate", new Dictionary<string, string>
                    {
                        {"legacy-mode", "True = Geração antiga | False = Geração via GPT-2"}
                    }},
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
