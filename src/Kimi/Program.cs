using System.Globalization;
using System.Reflection;
using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using Kimi.Commands;
using Kimi.Commands.Configuration;
using Kimi.Modules.Ranking;
using Kimi.Repository;
using Kimi.Repository.Repositories;
using Microsoft.EntityFrameworkCore;
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

        builder.Services.Configure<KimiConfiguration>(builder.Configuration.GetSection("Kimi"));

        var discordConfig = new DiscordSocketConfig
        {
            AlwaysDownloadUsers = true,
            MessageCacheSize = 100,
            GatewayIntents = (GatewayIntents)53542591
        };

        var interactionConfig = new InteractionServiceConfig
        {
            LocalizationManager = new ResxLocalizationManager("Kimi.Resources.Commands", Assembly.GetEntryAssembly(),
                new CultureInfo("en-US"), new CultureInfo("en-GB"), new CultureInfo("pt-BR")),
            InteractionCustomIdDelimiters = ['.']
        };

        builder.Logging.ClearProviders();
        builder.Logging.AddSerilog(dispose: true);

        builder.Services.AddDbContext<KimiDbContext>(options =>
            options.UseSqlite(builder.Configuration["ConnectionStrings:Database"]));

        builder.Services.AddScoped<GuildRepository>();
        builder.Services.AddScoped<UserRepository>();

        builder.Services.AddSingleton(discordConfig);
        builder.Services.AddSingleton<DiscordSocketClient>();
        builder.Services.AddSingleton<CommandService>();
        builder.Services.AddSingleton<CommandHandler>();
        builder.Services.AddSingleton(interactionConfig);
        builder.Services.AddSingleton<InteractionService>(x =>
            new InteractionService(x.GetRequiredService<DiscordSocketClient>().Rest, interactionConfig));
        builder.Services.AddSingleton<InteractionHandler>();

        builder.Services.AddScoped<RankingService>();

        builder.Services.AddSingleton<DiscordService>();

        builder.Services.AddHostedService<Worker>();

        var host = builder.Build();
        host.Run();
    }
}