using System.Text;
using Discord;
using Discord.Commands;
using Kimi.Commands.Configuration;
using Microsoft.Extensions.Options;
using InteractionService = Discord.Interactions.InteractionService;
using ModuleInfo = Discord.Interactions.ModuleInfo;

namespace Kimi.Commands.Meta;

[RequireOwner]
[Group("meta")]
public class MetaModule : ModuleBase<SocketCommandContext>
{
    private readonly InteractionService _interaction;

    private readonly KimiConfiguration _configuration;

    public MetaModule(InteractionService interaction, IOptions<KimiConfiguration> options)
    {
        _interaction = interaction;
        _configuration = options.Value;
    }

    [Command("ping")]
    public async Task Ping() => await ReplyAsync($"Pong! {Context.Client.Latency}ms");

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
            .WithThumbnailUrl("https://cdn.discordapp.com/attachments/354789360035561483/1341676355493953588/SCpink.png?ex=67b6dcfa&is=67b58b7a&hm=abbf0967972113576da8ab80fad1633970bb1384cae47c589d91150444a68877&")
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