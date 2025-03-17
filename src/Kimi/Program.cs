using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using Kimi.Commands;
using Kimi.Configuration;
using Kimi.Jobs;
using Kimi.Jobs.Configuration;
using Kimi.Modules.Birthday;
using Kimi.Modules.Ranking;
using Kimi.Repository;
using Kimi.Repository.Repositories;
using Microsoft.EntityFrameworkCore;
using Quartz;
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
        builder.Services.Configure<JobConfiguration>(builder.Configuration.GetSection("Jobs"));
        
        var discordConfig = new DiscordSocketConfig
        {
            AlwaysDownloadUsers = true,
            MessageCacheSize = 100,
            #if DEBUG
            GatewayIntents = GatewayIntents.All
            #else
            GatewayIntents = (GatewayIntents)53542591
            #endif
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
        builder.Services.AddScoped<ProfileRepository>();
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
        builder.Services.AddScoped<LevelService>();
        builder.Services.AddSingleton<BirthdayService>();

        builder.Services.AddSingleton<DiscordService>();

        builder.Services.AddSingleton<JobService>();
        builder.Services.AddQuartz();

        builder.Services.AddSingleton<Worker>();
        builder.Services.AddWindowsService(x => x.ServiceName = "Kimi");
        builder.Services.AddHostedService(x => x.GetRequiredService<Worker>());
        builder.Services.AddQuartzHostedService(x => x.WaitForJobsToComplete = true);

        var host = builder.Build();
        host.Run();
    }
}