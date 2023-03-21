using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog;
using Serilog.Formatting;
using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Xml.Schema;
using Kimi.Commands;
using Kimi.Commands.Modules.Utils;
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

            

            var config = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appconfig.json")
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

            await provider.GetRequiredService<LoggerService>().LoggerConfiguration(Environment.CurrentDirectory);

            var _client = provider.GetRequiredService<DiscordSocketClient>();
            var sCommands = provider.GetRequiredService<InteractionService>();

            var settings = provider.GetRequiredService<Settings>();

            var genSettings = settings.General;

            await provider.GetRequiredService<CommandHandler>().InitializeSlashAsync();
            await provider.GetRequiredService<CommandHandler>().InitializePrefixAsync();
            _client.Log += Log.Write;
            sCommands.Log += Log.Write;

            //SlashCommands slashCommands = new(_client);

            _client.Ready += async () =>
            {
                await Log.Write($"Revision {Info.Version}");
                
                var profile = settings.Profile;
                await _client.SetGameAsync(profile?.Status, profile?.Link, profile.ActivityType);
                await _client.SetStatusAsync(profile.UserStatus);

                await Log.Write(profile?.Status + profile?.ActivityType + profile?.UserStatus);

                
                if(settings.General.DebugGuildId != null)
                    foreach (var guild in settings.General.DebugGuildId)
                        await sCommands.RegisterCommandsToGuildAsync(guild, true);

                var state = new Commands.Modules.Utils.CommandInfo(sCommands);
                await Log.Write(await state.HandleSlashCommandsTable());

                await Log.Write($"Logged in as <@{_client.CurrentUser.Username}#{_client.CurrentUser.Discriminator}>!");
            };
            

            await Cache.LoadCacheFile();

            Cache cache = new();
            cache.CacheUpdate += async (sender, args) =>
            {
                var a = args.GenerationCache.Count;

                if(a > 3)
                    await Log.Write($"Current cache size: {args.GenerationCache.Count}", Severity.Verbose);
                else
                    await Log.Write($"Current cache size: {args.GenerationCache.Count}", Severity.Warning);
            };
            
            await Model.IsReady();

            //await .LoadTweets();

            await _client.LoginAsync(TokenType.Bot, Token.GetToken());

            await _client.StartAsync();

            //_client.SlashCommandExecuted += slashCommands.SlashCommandHandler;

            await Task.Delay(-1);
        }

        public void Brasil(object sender, CacheArgs args)
        {
        }
    }
}