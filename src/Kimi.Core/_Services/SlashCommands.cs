using Discord;
using Discord.Net;
using Discord.WebSocket;
using Kimi.Core._Modules;
using Kimi.Core._Modules.Monark;
using Kimi.Core._Services.Interfaces;

namespace Kimi.Core._Services
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
                //await Logging.LogAsync(exception.ToString(), Severity.Error);
            }
        }

        //public async Task SlashCommandHandler(SocketSlashCommand command)
        //{
        //    _ = command.Data.Name switch
        //    {
        //        "monark" => Monark.HandleSubCommands(command),
        //        "list-roles" => Placeholder.HandleListRoleCommand(command),
        //        _ => Logging.LogAsync($"<{command.Data.Name}> - {new NotImplementedException()}", Severity.Error)
        //    };

        //    await Task.CompletedTask;
        //}


    }
}
