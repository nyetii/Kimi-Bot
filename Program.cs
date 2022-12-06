using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using Kimi.Modules;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json.Linq;
using Serilog;
using Serilog.Events;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Kimi
{
    class Program
    {
        public static Task Main() => new Program().MainAsync();

        public async Task MainAsync()
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .Enrich.FromLogContext()
                .WriteTo.Console()
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
                    GatewayIntents = GatewayIntents.All,
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
            var config = provider.GetRequiredService<IConfigurationRoot>();
            var pCommands = provider.GetRequiredService<PrefixHandler>();
            await pCommands.InitializeAsync();

            await ModuleAdding.AddModule(host);

            _client.Log += LogAsync;
            sCommands.Log += LogAsync;

            _client.Ready += async () =>
            {
                Console.WriteLine("Ready!");
                await _client.SetGameAsync("poly", null, ActivityType.Watching);
                await _client.SetStatusAsync(UserStatus.Idle);
                await sCommands.RegisterCommandsGloballyAsync();
                Console.WriteLine(await Modules.Monark.MonarkSerialization.DeserializationAsync());

                Log.Information("Logged in as <{0}#{1}>!", _client.CurrentUser.Username, _client.CurrentUser.Discriminator);
            };

            #if !DEBUG
            await _client.LoginAsync(TokenType.Bot, config["Token"]);
            #else
            await _client.LoginAsync(TokenType.Bot, config["DebugToken"]);
            Log.Information("Running on the BETA account!");
            #endif


            await _client.StartAsync();

            await Task.Delay(-1);
        }

        private static async Task LogAsync(LogMessage message)
        {
            var severity = message.Severity switch
            {
                LogSeverity.Critical => LogEventLevel.Fatal,
                LogSeverity.Error => LogEventLevel.Error,
                LogSeverity.Warning => LogEventLevel.Warning,
                LogSeverity.Info => LogEventLevel.Information,
                LogSeverity.Verbose => LogEventLevel.Verbose,
                LogSeverity.Debug => LogEventLevel.Debug,
                _ => LogEventLevel.Information
            };
            Log.Write(severity, message.Exception, "[{Source}] {Message}", message.Source, message.Message);
            await Task.CompletedTask;
        }
    }

    class ModuleAdding : Program
    {
        public static async Task AddModule(IHost host)
        {
            using IServiceScope serviceScope = host.Services.CreateScope();
            IServiceProvider provider = serviceScope.ServiceProvider;
            var pCommands = provider.GetRequiredService<PrefixHandler>();

            // Add modules here
            pCommands.AddModule<PrefixModule>();
            pCommands.AddModule<Modules.Monark.Monark>();
            pCommands.AddModule<State>();
            pCommands.AddModule<Modules.Info.Help>();

            await Task.CompletedTask;
        }
    }
}
