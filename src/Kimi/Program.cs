using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using Kimi.Commands;
using Kimi.Modules.Ranking;
using Serilog;

namespace Kimi;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);

        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(builder.Configuration)
            .CreateLogger();

        var discordConfig = new DiscordSocketConfig
        {
            AlwaysDownloadUsers = true,
            MessageCacheSize = 100,
            GatewayIntents = GatewayIntents.All
        };

        builder.Logging.ClearProviders();
        builder.Logging.AddSerilog(dispose: true);

        builder.Services.AddSingleton(discordConfig);
        builder.Services.AddSingleton<DiscordSocketClient>();
        //builder.Services.AddSingleton<InteractionService>();
        builder.Services.AddSingleton<CommandService>();
        builder.Services.AddSingleton<CommandHandler>();

        builder.Services.AddSingleton<RankingService>();

        builder.Services.AddHostedService<Worker>();

        var host = builder.Build();
        host.Run();
    }
}