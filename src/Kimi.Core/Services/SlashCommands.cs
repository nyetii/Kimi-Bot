using Discord;
using Discord.Net;
using Discord.WebSocket;
using Kimi.Core.Modules.Monark;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Kimi.Core.Services.Interfaces;
using static System.Net.Mime.MediaTypeNames;

namespace Kimi.Core.Services
{
    internal class SlashCommands
    {
        private readonly DiscordSocketClient _client;

        public SlashCommands(DiscordSocketClient client)
        {
            _client = client;
        }

        public async Task HandleSlashCommands()
        {
            const ulong guildId = 973401092274659358;

            var guildCommand = new SlashCommandBuilder()
            .WithName("list-roles")
            .WithDescription("Lists all roles of a user.")
            .AddOption("user", ApplicationCommandOptionType.User, "The users whos roles you want to be listed", isRequired: true);

            ICommandInitializer commandInitializer = new MonarkInitialize(_client);
            await commandInitializer.Initialize();

            //await new MonarkInitialize(_client).Initialize();

            //var monarkCommands = new SlashCommandBuilder()
            //.WithName("monark")
            //.WithDescription("Gerar um tweet aleatório do Monark")
            //.AddOption(new SlashCommandOptionBuilder()
            //    .WithName("generate")
            //    .WithDescription("Gerar um tweet aleatório do Monark")
            //    .WithType(ApplicationCommandOptionType.SubCommand))
            //.AddOption(new SlashCommandOptionBuilder()
            //    .WithName("force")
            //    .WithDescription("Gerar um tweet")
            //    .WithType(ApplicationCommandOptionType.SubCommand)
            //    .AddOption(new SlashCommandOptionBuilder()
            //        .WithName("tweet")
            //        .WithDescription("Ex.: Olá Mundo!")
            //        .WithType(ApplicationCommandOptionType.String)
            //        .WithMaxLength(280)
            //        .WithMinLength(1)
            //        .WithRequired(true))
            //    .AddOption(new SlashCommandOptionBuilder()
            //        .WithName("image")
            //        .WithDescription("URL de uma imagem")
            //        .WithType(ApplicationCommandOptionType.Attachment))
            //    .AddOption(new SlashCommandOptionBuilder()
            //        .WithName("avatar")
            //        .WithDescription("URL de uma imagem")
            //        .WithType(ApplicationCommandOptionType.Attachment))
            //    .AddOption(new SlashCommandOptionBuilder()
            //        .WithName("username")
            //        .WithDescription("Nome de usuário")
            //        .WithType(ApplicationCommandOptionType.String)
            //        .WithMaxLength(15)
            //        .WithMinLength(3))
            //    .AddOption(new SlashCommandOptionBuilder()
            //        .WithName("nickname")
            //        .WithDescription("Nome de usuário")
            //        .WithType(ApplicationCommandOptionType.String)
            //        .WithMaxLength(50)
            //        .WithMinLength(4)))
            //.AddOption(new SlashCommandOptionBuilder()
            //    .WithName("count")
            //    .WithDescription("Conta a quantidade de tweets do Monark")
            //    .WithType(ApplicationCommandOptionType.SubCommand));

            try
            {
                await _client.Rest.CreateGuildCommand(guildCommand.Build(), guildId);
                //await _client.Rest.CreateGuildCommand(monarkCommands.Build(), guildId);
            }
            catch (ApplicationCommandException exception)
            {
                await Logging.LogAsync(exception.ToString(), Severity.Error);
            }
        }

        public async Task SlashCommandHandler(SocketSlashCommand command)
        {
            _ = command.Data.Name switch
            {
                "list-roles" => Modules.Placeholder.HandleListRoleCommand(command),
                "monark" => Modules.Monark.Monark.HandleCommandGroup(command),
                _ => Logging.LogAsync(new NotImplementedException().ToString(), Severity.Error)
            };

            await Task.CompletedTask;
            // Let's add a switch statement for the command name so we can handle multiple commands in one event.
            //switch (command.Data.Name)
            //{
            //    case "list-roles":
            //        await HandleListRoleCommand(command);
            //        break;
            //}
        }


    }
}
