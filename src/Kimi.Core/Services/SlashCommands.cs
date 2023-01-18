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
            await commandInitializer.CreateCommand();

            try
            {
                //await _client.Rest.CreateGlobalCommand(abc.Result.Build());
                //await _client.Rest.CreateGuildCommand(guildCommand.Build(), guildId);
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
                "monark" => Modules.Monark.Monark.HandleSubCommands(command),
                "list-roles" => Modules.Placeholder.HandleListRoleCommand(command),
                _ => Logging.LogAsync(new NotImplementedException().ToString(), Severity.Error)
            };

            await Task.CompletedTask;
        }


    }
}
