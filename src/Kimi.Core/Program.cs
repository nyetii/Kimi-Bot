using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using Kimi.Core.Services;
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

namespace Kimi.Core
{
    internal class Program
    {
        public static Task Main() => new Program().MainAsync();

        public async Task MainAsync()
        {
            Debug.Assert(Info.IsDebug = true);

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .WriteTo.File(@$"{Info.AppDataPath}\logs\kimi.log", rollingInterval: RollingInterval.Day, 
                outputTemplate: "[{Timestamp:yyyy-MM-dd - HH:mm:ss.fff}|{Level:u3}] {Message:lj}{NewLine}{Exception}")
                .CreateLogger();

            var config = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appconfig.json")
                .Build();

            using IHost host = Host.CreateDefaultBuilder()
                .ConfigureServices((_, services) =>
                services
                .AddSingleton(config)
                .AddSingleton(x => new DiscordSocketClient(new DiscordSocketConfig
                {
                    GatewayIntents = Discord.GatewayIntents.All,
                    AlwaysDownloadUsers = false
                }))
                .AddSingleton(x => new InteractionService(x.GetRequiredService<DiscordSocketClient>()))
                .AddSingleton<InteractionHandler>() 
                .AddSingleton(x => new CommandService())
                .AddSingleton<PrefixHandler>())
                .Build();

            await RunAsync(host);
        }

        public async Task RunAsync(IHost host)
        {
            using IServiceScope serviceScope = host.Services.CreateScope();
            IServiceProvider provider = serviceScope.ServiceProvider;

            var _client = provider.GetRequiredService<DiscordSocketClient>();
            var sCommands = provider.GetRequiredService<InteractionService>();

            await provider.GetRequiredService<InteractionHandler>().InitializeAsync();
            await provider.GetRequiredService<PrefixHandler>().InitializeAsync();

            _client.Log += Logging.LogAsync;
            sCommands.Log += Logging.LogAsync;


            _client.Ready += async () =>
            {
                Settings? settings = new Settings();
                KimiData.LoadSettings();

                await Logging.LogAsync($"Revision {Info.Version}");
                
                var profile = settings.Profile;
                await _client.SetGameAsync(profile?.Status, profile?.Link, profile.ActivityType);
                await _client.SetStatusAsync(profile.UserStatus);
                await sCommands.RegisterCommandsGloballyAsync();

                await Logging.LogAsync($"Logged in as <@{_client.CurrentUser.Username}#{_client.CurrentUser.Discriminator}>!");
            };

            await _client.LoginAsync(TokenType.Bot, Token.GetToken());

            await _client.StartAsync();

            await Task.Delay(-1);
        }
    }
}