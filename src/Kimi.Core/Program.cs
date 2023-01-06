using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using System.Reflection;

namespace Kimi.Core
{
    internal class Program
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
                    GatewayIntents = Discord.GatewayIntents.All,
                    AlwaysDownloadUsers = false
                }))
                .AddSingleton(x => new InteractionService(x.GetRequiredService<DiscordSocketClient>()))
                //.AddSingleton<InteractionHandler>() 
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
            //await provider.GetRequiredService<InteractionHandler>().InitializeAsync();
            var config = provider.GetRequiredService<IConfigurationRoot>();
            var pCommands = provider.GetRequiredService<PrefixHandler>();
            await pCommands.InitializeAsync();

            //await ModuleAdding.AddModule(host);

            //_client.Log += LogAsync;
            //sCommands.Log += LogAsync;

            _client.Ready += async () =>
            {
                Console.WriteLine(Assembly.GetEntryAssembly().GetName().Version.ToString());
                Console.WriteLine("Ready!");
                await _client.SetGameAsync("PORN HUB GAY", null, Discord.ActivityType.Watching);
                await _client.SetStatusAsync(Discord.UserStatus.Idle);
                await sCommands.RegisterCommandsGloballyAsync();

                Log.Information("Logged in as <{0}#{1}>!", _client.CurrentUser.Username, _client.CurrentUser.Discriminator);
            };

            await _client.LoginAsync(TokenType.Bot, config["Token"]);

            await _client.StartAsync();

            await Task.Delay(-1);
        }
    }
}