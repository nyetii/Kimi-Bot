using Discord;
using Discord.Net;
using Discord.WebSocket;
using Kimi.Logging;
using Kimi.Services.Commands.Interfaces;

namespace Kimi.Commands
{
    public class SlashCommands
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

            //ICommandInitializer commandInitializer = new MonarkInitialize(_client);
            //await commandInitializer.Initialize();
            //await commandInitializer.CreateCommand();

            try
            {
                //await _client.Rest.CreateGlobalCommand(abc.Result.Build());
                //await _client.Rest.CreateGuildCommand(guildCommand.Build(), guildId);
            }
            catch (ApplicationCommandException exception)
            {
                await Log.Write(exception.ToString(), Severity.Error);
            }
        }

        public async Task SlashCommandHandler(SocketSlashCommand command)
        {
            //_ = command.Data.Name switch
            //{
                //"monark" => Modules.Monark.Monark.HandleSubCommands(command),
                //"list-roles" => Modules.Placeholder.HandleListRoleCommand(command),
                //_ => Logging.LogAsync($"<{command.Data.Name}> - {new NotImplementedException()}", Severity.Error)
            //};

            await Task.CompletedTask;
        }


    }
}
