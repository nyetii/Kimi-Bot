﻿using Discord;
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
using System.Xml.Schema;
using Kimi.GPT2;

namespace Kimi.Core
{
    internal class Program
    {
        public static Task Main() => new Program().MainAsync();

        public async Task MainAsync()
        {
            Debug.Assert(Info.IsDebug = true);

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Verbose()
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
                    AlwaysDownloadUsers = false,
                }))
                .AddSingleton(x => new InteractionService(x.GetRequiredService<DiscordSocketClient>()))
                .AddSingleton<InteractionHandler>() 
                .AddSingleton(x => new CommandService())
                .AddSingleton<Kimi.Commands.PrefixHandler>())
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
            await provider.GetRequiredService<Kimi.Commands.PrefixHandler>().InitializeAsync();

            _client.Log += Logging.LogAsync;
            sCommands.Log += Logging.LogAsync;

            SlashCommands slashCommands = new(_client);

            _client.Ready += async () =>
            {
                
                
                Settings? settings = new Settings();
                settings = await KimiData.LoadSettings();

                await Logging.LogAsync($"Revision {Info.Version}");
                
                var profile = settings.Profile;
                await _client.SetGameAsync(profile?.Status, profile?.Link, profile.ActivityType);
                await _client.SetStatusAsync(profile.UserStatus);

                await Logging.LogAsync(profile?.Status + profile?.ActivityType + profile?.UserStatus);

                
                await slashCommands.HandleSlashCommands();
                //await sCommands.AddCommandsGloballyAsync();

                await Logging.LogAsync($"Logged in as <@{_client.CurrentUser.Username}#{_client.CurrentUser.Discriminator}>!");
            };
            

            await Cache.LoadCacheFile();

            Cache cache = new();
            cache.CacheUpdate += async (sender, args) =>
            {
                var a = args.GenerationCache.Count;

                if(a > 3)
                    await Logging.LogAsync($"Current cache size: {args.GenerationCache.Count}", Severity.Verbose);
                else
                    await Logging.LogAsync($"Current cache size: {args.GenerationCache.Count}", Severity.Warning);
            };
            
            await Model.IsReady();

            await KimiData.LoadTweets();

            await _client.LoginAsync(TokenType.Bot, Token.GetToken());

            await _client.StartAsync();

            _client.SlashCommandExecuted += slashCommands.SlashCommandHandler;

            await Task.Delay(-1);
        }

        public void Brasil(object sender, CacheArgs args)
        {
        }
    }
}