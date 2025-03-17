using System.Text;
using Discord;
using Discord.Commands;
using Kimi.Configuration;
using Kimi.Jobs;
using Microsoft.Extensions.Options;
using InteractionService = Discord.Interactions.InteractionService;
using ModuleInfo = Discord.Interactions.ModuleInfo;

namespace Kimi.Commands.Meta;

[Group("meta")]
public class MetaModule : ModuleBase<SocketCommandContext>
{
    private readonly InteractionService _interaction;

    private readonly KimiConfiguration _configuration;
    private readonly JobService _jobService;
    private readonly Worker _worker;

    public MetaModule(InteractionService interaction, IOptions<KimiConfiguration> options, JobService jobService,
        Worker worker)
    {
        _interaction = interaction;
        _jobService = jobService;
        _worker = worker;
        _configuration = options.Value;
    }

    [Command("ping")]
    public async Task Ping() => await ReplyAsync($"Pong! {Context.Client.Latency}ms");

    [RequireOwner]
    [Command("react")]
    public async Task React(ulong messageId, [Remainder] string emoteStr)
    {
        var message = await Context.Channel.GetMessageAsync(messageId);

        if (message is null)
        {
            await ReplyAsync("Invalid message ID.");
            return;
        }
        
        if (Emote.TryParse(emoteStr, out var emote))
        {
            await message.AddReactionAsync(emote);
            return;
        }

        if (Emoji.TryParse(emoteStr, out var emoji))
        {
            await message.AddReactionAsync(emoji);
            return;
        }
        
        await ReplyAsync($"{emoteStr} is not a valid emote.");
        
    }

    [RequireOwner]
    [Command("force-trigger")]
    public async Task ForceTrigger([Remainder] string jobName)
    {
        try
        {
            await _jobService.ForceTriggerAsync(jobName);
            await ReplyAsync("Triggered!");
        }
        catch (Exception ex)
        {
            await ReplyAsync($"Could not force trigger.\n{ex.Message}");
            throw;
        }
    }

    [RequireOwner]
    [Command("shutdown")]
    public async Task Shutdown()
    {
        await ReplyAsync("Shutting down...");
        await _worker.StopAsync(CancellationToken.None);
    }

    [RequireOwner]
    [Command("fetch-roles")]
    public async Task FetchRoles(string? topRole = null, string? bottomRole = null)
    {
        var top = await Context.Guild.GetRoleAsync(ulong.Parse(topRole ?? "0"));
        var bottom = await Context.Guild.GetRoleAsync(ulong.Parse(bottomRole ?? "0"));

        var roles = Context.Guild.Roles.Where(x
            => x.Position < (top?.Position ?? Context.Guild.Roles.Max(m => m.Position))
               && x.Position > (bottom?.Position ?? 0)).OrderByDescending(x => x.Position);

        var embedBuilder = new EmbedBuilder();
        embedBuilder.WithColor(243, 197, 199);
        embedBuilder.WithAuthor("Embed to make it pretty!");
        embedBuilder.WithTitle("Fetched Roles");
        embedBuilder.WithThumbnailUrl(
            "https://cdn.discordapp.com/attachments/354786508550569994/1343786350285033472/ezgif-67a72d80be1893.gif?ex=67be8a10&is=67bd3890&hm=813bd852d29227b94fdd85ccdd2b5c97c50db8b2bd1936cea0c9f2cf1bb9fba9&");
        foreach (var role in roles)
        {
            embedBuilder.AddField(role.Name, $"[Icon]({role.GetIconUrl()}) - {role.Color.ToString().ToUpper()}");
        }

        embedBuilder.WithFooter("<3", Context.User.GetAvatarUrl());
        embedBuilder.WithCurrentTimestamp();

        await ReplyAsync(embed: embedBuilder.Build());
    }

    [RequireOwner]
    [Command("refresh")]
    public async Task RefreshModules()
    {
        await _interaction.RegisterCommandsToGuildAsync(Context.Guild.Id);

        var guild = _configuration.Guilds.FirstOrDefault(g => g.Id == Context.Guild.Id);

        if (guild is null)
        {
            await ReplyAsync("This guild is not registered.");
            return;
        }

        var modules = _interaction.Modules.Where(x => x.DontAutoRegister && CheckModule(x, guild)).ToArray();

        await _interaction.AddModulesToGuildAsync(Context.Guild.Id, true, modules);

        await ReplyAsync("Refreshed modules.");
    }

    [Command("modules")]
    public async Task Modules()
    {
        var guild = _configuration.Guilds.FirstOrDefault(g => g.Id == Context.Guild.Id);

        if (guild is null)
        {
            await ReplyAsync("This guild is not registered.");
            return;
        }

        var modules = _interaction.Modules.Where(x => x.DontAutoRegister).ToArray();

        var commandsStr = new StringBuilder();
        foreach (var module in modules)
        {
            var enabled = CheckModule(module, guild);
            commandsStr.AppendLine($"- **{module.SlashGroupName}**");
            commandsStr.AppendLine($"  - *{(enabled ? "enabled" : "disabled")}*");
        }

        var embed = new EmbedBuilder()
            .WithColor(243, 197, 199)
            .WithAuthor("Meta", Context.Guild.IconUrl)
            .WithThumbnailUrl(
                "https://cdn.discordapp.com/attachments/354789360035561483/1341676355493953588/SCpink.png?ex=67b6dcfa&is=67b58b7a&hm=abbf0967972113576da8ab80fad1633970bb1384cae47c589d91150444a68877&")
            .WithTitle("Modules")
            .WithDescription("Status of the modules in this server")
            .WithFields(modules.Select(module
                => new EmbedFieldBuilder().WithName(module.SlashGroupName)
                    .WithValue(CheckModule(module, guild) ? "*enabled*" : "*disabled*")
                    .WithIsInline(true)))
            .WithFooter("Kimi", Context.Client.CurrentUser.GetAvatarUrl())
            .Build();

        await ReplyAsync(embed: embed);
    }

    private bool CheckModule(ModuleInfo module, GuildConfiguration guildConfig)
    {
        return guildConfig.Modules[module.SlashGroupName];
    }
}