using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Xml.Schema;
using Kimi.Commands;
using Kimi.GPT2;
using Kimi.Logging;
using Kimi.Services.Core;
using Info = Kimi.Services.Core.Info;
using KimiData = Kimi.Services.Core.KimiData;
using Log = Kimi.Logging.Log;
using Settings = Kimi.Services.Core.Settings;

namespace Kimi.Core
{
    internal class Program
    {
        public static Task Main() => new Program().MainAsync();

        public async Task MainAsync()
        {
            Debug.Assert(Info.IsDebug = true);

            Console.Title = Info.IsDebug ? "Kimi [DEBUG]" : "Kimi";

            var config = new ConfigurationBuilder()
                .Build();

            using IHost host = Host.CreateDefaultBuilder()
                .ConfigureServices((_, services) =>
                services
                .AddSingleton(config)
                .AddSingleton<KimiData>()
                .AddSingleton<Settings>(x => x.GetRequiredService<KimiData>().LoadSettings())
                .AddSingleton(x => new DiscordSocketClient(new DiscordSocketConfig
                {
                    GatewayIntents = Discord.GatewayIntents.All,
                    AlwaysDownloadUsers = false,
                }))
                .AddSingleton(x => new InteractionService(x.GetRequiredService<DiscordSocketClient>()))
                .AddSingleton(x => new CommandService())
                .AddSingleton<CommandHandler>()
                .AddSingleton<LoggerService>())
                .Build();

            await RunAsync(host);
        }

        public async Task RunAsync(IHost host)
        {
            using IServiceScope serviceScope = host.Services.CreateScope();
            IServiceProvider provider = serviceScope.ServiceProvider;

            await provider.GetRequiredService<LoggerService>().LoggerConfiguration(Info.AppDataPath);

            var _client = provider.GetRequiredService<DiscordSocketClient>();
            var sCommands = provider.GetRequiredService<InteractionService>();

            var settings = provider.GetRequiredService<Settings>();

            await provider.GetRequiredService<CommandHandler>().InitializeSlashAsync();
            await provider.GetRequiredService<CommandHandler>().InitializePrefixAsync();
            _client.Log += Log.Write;
            sCommands.Log += Log.Write;

            _client.Ready += async () =>
            {
                await Log.Write($"Revision {Info.Version}");
                
                var profile = settings.Profile;
                await _client.SetGameAsync(profile?.Status, profile?.Link, profile.ActivityType);
                await _client.SetStatusAsync(profile.UserStatus);

                var state = new Commands.Modules.Utils.CommandInfo(sCommands);
                await Log.Write(await state.HandleSlashCommandsTable());

                await Log.Write($"Logged in as <@{_client.CurrentUser.Username}#{_client.CurrentUser.Discriminator}>!");
                await Log.Write($"{profile.UserStatus} - {profile.ActivityType} {profile.Status}");
            };

            await Model.IsReady();

            await _client.LoginAsync(TokenType.Bot, Token.GetToken());

            await _client.StartAsync();

            await Task.Delay(-1);
        }
    }
}