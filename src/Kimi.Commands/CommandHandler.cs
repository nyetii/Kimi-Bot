using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Discord.Interactions;
using Kimi.Logging;
using Kimi.Services.Core;

namespace Kimi.Commands
{
    public class CommandHandler
    {
        public ulong? GuildId { get; init; }
        private readonly string[] _prefix;
        private readonly DiscordSocketClient _client;
        private readonly CommandService _commands;
        private readonly IConfigurationRoot _config;
        private readonly IServiceProvider _services;
        private readonly InteractionService _slash;

        public CommandHandler(Settings settings, DiscordSocketClient client, CommandService commands, InteractionService slash, IConfigurationRoot config, IServiceProvider services)
        {
            GuildId = settings.General.DebugGuildId;
            _prefix = settings.General.Prefix;
            _client = client;
            _commands = commands;
            _slash = slash;
            _config = config;
            _services = services;
        }

        public async Task InitializePrefixAsync()
        {
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
            _client.MessageReceived += HandleCommandAsync;
            _client.SlashCommandExecuted += async (command) =>
            {
                _ = command.Data.Name switch
                {
                    //"monark" => Modules.Monark.Monark.HandleSubCommands(command),
                    //"list-roles" => Modules.Placeholder.HandleListRoleCommand(command),
                    _ => Log.Write($"<{command.Data.Name}> - {new NotImplementedException()}", Severity.Error)
                };
                await Task.CompletedTask;
            };
        }

        private async Task HandleCommandAsync(SocketMessage messageParam)
        {
            var message = messageParam as SocketUserMessage;
            if (message == null) return;

            int argPos = 0;

            bool hasPrefix = _prefix.Any(prefix => message.HasStringPrefix(prefix, ref argPos));
                
            if(!hasPrefix && !message.HasMentionPrefix(_client.CurrentUser, ref argPos))
                return;

            var context = new SocketCommandContext(_client, message);

            await _commands.ExecuteAsync(
                context: context,
                argPos: argPos,
                services: _services);
        }

        public async Task InitializeSlashAsync()
        {
            await _slash.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
            _client.InteractionCreated += HandleInteractionAsync;
        }

        private async Task HandleInteractionAsync(SocketInteraction arg)
        {
            try
            {
                var ctx = new SocketInteractionContext(_client, arg);
                await _slash.ExecuteCommandAsync(ctx, _services);
            }
            catch (Exception ex)
            {
                //await Logging.LogAsync(ex.ToString());
            }
        }
    }
}
