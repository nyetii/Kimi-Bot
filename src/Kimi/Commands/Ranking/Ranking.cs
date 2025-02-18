using Discord;
using Discord.Interactions;
using Kimi.Repository.Dtos;
using Kimi.Repository.Repositories;

namespace Kimi.Commands.Ranking;

[Group("ranking", "Leaderboard commands")]
public class Ranking : InteractionModuleBase<SocketInteractionContext>
{
    private readonly ILogger<Ranking> _logger;
    private readonly GuildRepository _guildRepository;

    public Ranking(ILogger<Ranking> logger, IServiceProvider provider)
    {
        _logger = logger;
        _guildRepository = provider.CreateScope().ServiceProvider.GetRequiredService<GuildRepository>();
    }

    #region Commands

    [SlashCommand("top", "Lists the top 10 users by score")]
    public async Task RankingTopCommand(
        [Summary(description: "Selects page - eg. page 3 for the 21st to 30th place")] [MinValue(1)]
        int page = 1,
        [Summary(description: "The response will only be visible to you")]
        bool ephemeral = false)
    {
        page--;

        await DeferAsync(ephemeral);

        var result = await HandleLeaderboardAsync(page);

        if (result.Embed is null)
        {
            await FollowupAsync("This page doesn't exist.");
            return;
        }

        await FollowupAsync(embed: result.Embed, components: result.Component, ephemeral: ephemeral);
    }

    [ComponentInteraction("paging:*")]
    public async Task RankingTopPagingButton(int page)
    {
        await DeferAsync();

        var result = await HandleLeaderboardAsync(page);

        if (result.Embed is null)
        {
            await DeleteOriginalResponseAsync();
            await FollowupAsync("This page doesn't exist.");
            return;
        }

        await ModifyOriginalResponseAsync(x =>
        {
            x.Embed = result.Embed;
            x.Components = result.Component;
        });
    }

    #endregion

    #region Handling

    private async Task<(Embed? Embed, MessageComponent? Component)> HandleLeaderboardAsync(int page)
    {
        var totalScores = await _guildRepository.GetAllTotalScoresAsync(Context.Guild.Id);

        var scores = totalScores.Skip(page * 10).Take(10).ToList();

        if (scores.Count is 0)
            scores = totalScores.TakeLast(10).ToList();

        if (scores.Count is 0)
            return (null, null);

        var callerIndex = totalScores.FindIndex(x => x.UserId == Context.User.Id) + 1;

        var scoreboard = new
        {
            Names = scores.Select(x => x.Nickname).ToArray(),
            Scores = scores.Select(x => x.Score.ToString()).ToArray(),
            MessageCounts = scores.Select(x => x.MessageCount.ToString()).ToArray(),
        };

        var embedBuilder = new EmbedBuilder();

        embedBuilder
            .WithColor(243, 197, 199)
            .WithAuthor("All time", Context.Guild.IconUrl)
            .WithTitle($"{Context.Guild.Name} leaderboard")
            .WithDescription($"You're on the **{GetOrdinalString(callerIndex)} place**.");

        embedBuilder.AddField("Position", BuildPositions(scoreboard.Names, page), true);
        embedBuilder.AddField("Score", BuildScores(scoreboard.Scores), true);
        embedBuilder.AddField("Messages", BuildTotalMessages(scoreboard.MessageCounts), true);

        var lastPosition = (page + 1) * 10 > totalScores.Count
            ? totalScores.Count
            : (page + 1) * 10;

        embedBuilder
            .WithThumbnailUrl("https://cdn.discordapp.com/emojis/1287710330956419156.webp?size=96")
            .WithFooter($"Positions {page * 10 + 1}-{lastPosition} from {totalScores.Count}",
                "https://cdn.discordapp.com/emojis/783328274193448981.webp?size=96")
            .WithCurrentTimestamp();

        var embed = embedBuilder.Build();

        var componentBuilder = new ComponentBuilder();

        if (page is not 0)
            componentBuilder
                .WithButton($"Page {page}", style: ButtonStyle.Secondary, customId: $"ranking.paging:{page - 1}");

        if (page * 10 > totalScores.Count)
            componentBuilder
                .WithButton($"Page {page + 2}", style: ButtonStyle.Secondary, customId: $"ranking.paging:{page + 1}");

        componentBuilder.WithButton(" ", style: ButtonStyle.Secondary, customId: $"ranking.paging:{page + 2}",
            emote: Emote.Parse("<:nazubeans:783328274193448981>"));

        return (embed, componentBuilder.Build());
    }

    private string BuildPositions(string[] names, int page = 0)
    {
        var str = string.Empty;

        for (var i = 0; i < names.Length; i++)
        {
            var formattedName = names[i] + "**";
            if (names[i].Length > 16)
                formattedName = formattedName[..16] + "**…";

            var position = GetOrdinalString(i + 1 + page * 10).PadRight(4);

            str += $"**`{position}`\u2002{formattedName}\n";
        }

        return str;
    }

    private string BuildScores(string[] scores)
    {
        var str = string.Empty;

        var maxLength = scores.Max(x => x.Length);

        foreach (var score in scores)
            str += $"`{score.PadRight(maxLength)}`\n";

        return str;
    }

    private string BuildTotalMessages(string[] totalMessagesByUser)
    {
        var str = string.Empty;

        var maxLength = totalMessagesByUser.Max(x => x.Length);

        foreach (var totalMessages in totalMessagesByUser)
            str += $"`{totalMessages.PadRight(maxLength)}`\n";

        return str;
    }

    private string GetOrdinalString(int number)
    {
        return (number % 100) switch
        {
            11 or 12 or 13 => $"{number}th",
            _ => (number % 10) switch
            {
                1 => $"{number}st",
                2 => $"{number}nd",
                3 => $"{number}rd",
                _ => $"{number}th"
            }
        };
    }

    #endregion
}