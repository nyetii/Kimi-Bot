using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using Kimi.Core.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Serilog;
using System.Diagnostics;
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

            _client.Log += Logging.LogAsync;
            sCommands.Log += Logging.LogAsync;


            _client.Ready += async () =>
            {
                Settings? settings = new Settings();
                var path = $@"{Info.AppDataPath}\settings.kimi";

                if(!File.Exists(path))
                    using(StreamWriter sw = new StreamWriter(path))
                    using(JsonWriter writer = new JsonTextWriter(sw))
                    {
                        Log.Information("[{Source}] Settings file doesn't exist, creating default file...", "Kimi");
                        writer.Formatting = Formatting.Indented;
                        JsonSerializer serializer = new JsonSerializer();
                        serializer.Serialize(writer, settings);
                    }

                path = File.ReadAllText(path);
                settings = JsonConvert.DeserializeObject<Settings>(path);
                Log.Information("[{Source}] Settings loaded!", "Kimi");
                var profile = settings.Profile;

                var attribute = (AssemblyInformationalVersionAttribute)Assembly.GetExecutingAssembly()
                .GetCustomAttributes(typeof(AssemblyInformationalVersionAttribute), false)
                .FirstOrDefault();

                Log.Information("[{Source}] Revision {Version}", "Kimi", Info.Version);

                Console.WriteLine("Ready!");
                await _client.SetGameAsync(profile?.Status, profile?.Link, profile.ActivityType);
                await _client.SetStatusAsync(profile.UserStatus);
                await sCommands.RegisterCommandsGloballyAsync();

                Log.Information("[{Source}] Logged in as <{0}#{1}>!", "Kimi", _client.CurrentUser.Username, _client.CurrentUser.Discriminator);
            };

            await _client.LoginAsync(TokenType.Bot, Token.GetToken());

            await _client.StartAsync();

            await Task.Delay(-1);
        }
    }
}