﻿using System.Numerics;
using Discord;
using Discord.Interactions;
using Kimi.Extensions;
using Kimi.Repository.Dtos;
using Kimi.Repository.Repositories;

namespace Kimi.Commands.Ranking;

[DontAutoRegister]
[Group("ranking", "Leaderboard commands")]
public class RankingModule : InteractionModuleBase<SocketInteractionContext>
{
    private readonly ILogger<RankingModule> _logger;
    private readonly GuildRepository _guildRepository;

    public RankingModule(ILogger<RankingModule> logger, IServiceProvider provider)
    {
        _logger = logger;
        _guildRepository = provider.CreateScope().ServiceProvider.GetRequiredService<GuildRepository>();
    }

    #region Commands

    #region Top

    [SlashCommand("top", "Lists the top 10 users by score")]
    public async Task RankingTopCommand(
        [Summary(description: "Selects page - eg. page 3 for the 21st to 30th place")] [MinValue(1)]
        int page = 1,
        [Summary("order-by", description: "Selects what order to show (score or message count)")]
        OrderType orderBy = OrderType.Score,
        [Summary(description: "Selects the time period")]
        PeriodType period = PeriodType.AllTime,
        [Summary(description: "The response will only be visible to you")]
        bool ephemeral = false)
    {
        page--;

        await DeferAsync(ephemeral);

        var result = await HandleLeaderboardAsync(page, orderBy, new PeriodDto(period));

        if (result.Embed is null)
        {
            await FollowupAsync("This page doesn't exist.");
            return;
        }

        await FollowupAsync(embed: result.Embed, components: result.Component, ephemeral: ephemeral);

        if (result.Component is not null)
            await DisableButtonsAsync(result.Component);
    }

    [SlashCommand("top-period", "Lists the top 10 users by score during a certain period")]
    public async Task RankingTopPeriodCommand(
        [Summary(description: "The starting date. Formatting: dd/mm/yyyy or yyyy/mm/dd")]
        DateTime startDate,
        [Summary(description: "The ending date. If no date is provided, today will be considered the ending date")]
        DateTime? endDate = null,
        [Summary(description: "Selects page - eg. page 3 for the 21st to 30th place")] [MinValue(1)]
        int page = 1,
        [Summary("order-by", description: "Selects what order to show (score or message count)")]
        OrderType orderBy = OrderType.Score,
        [Summary(description: "The response will only be visible to you")]
        bool ephemeral = false)
    {
        page--;

        await DeferAsync(ephemeral);


        if (endDate < startDate)
        {
            await FollowupAsync("Invalid date range, the starting date provided is greater than the ending date.");
            return;
        }

        var result = await HandleLeaderboardAsync(page, orderBy,
            new PeriodDto(startDate, endDate ?? DateTime.UtcNow.Date));

        if (result.Embed is null)
        {
            await FollowupAsync("This page doesn't exist.");
            return;
        }

        await FollowupAsync(embed: result.Embed, components: result.Component, ephemeral: ephemeral);

        if (result.Component is not null)
            await DisableButtonsAsync(result.Component);
    }

    [ComponentInteraction("paging:*,*,*,*,*")]
    public async Task RankingTopPagingButton(int page, OrderType order, DateTime start, DateTime end, PeriodType type)
    {
        await DeferAsync();

        var period = end == DateTime.MinValue
            ? new PeriodDto(type)
            : new PeriodDto(start, end, type);

        var result = await HandleLeaderboardAsync(page, order, period);

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

    private async Task DisableButtonsAsync(MessageComponent messageComponent)
    {
        await Task.Delay(TimeSpan.FromMinutes(5));

        var componentBuilder = new ComponentBuilder();
        foreach (var component in messageComponent.Components)
        {
            var row = new ActionRowBuilder();
            foreach (var button in component.Components.Cast<ButtonComponent>())
            {
                row.AddComponent(button.ToBuilder().WithDisabled(true).Build());
            }

            componentBuilder.AddRow(row);
        }

        await ModifyOriginalResponseAsync(x => { x.Components = componentBuilder.Build(); });
    }

    #endregion

    #region Info

    [SlashCommand("info", "Gets interesting info of the server's ranking")]
    public async Task RankingInfoCommand(bool ephemeral = false)
    {
        await DeferAsync(ephemeral);

        var guild = await _guildRepository.GetAsync(Context.Guild.Id);

        var startDate = guild.DailyScores.Min(x => x.Date);

        var totalScore = guild.DailyScores.Sum(x => x.Score);
        var totalMessages = guild.DailyScores.Sum(x => x.MessageCount);

        var dailyScoresByUser = guild.DailyScores
            .GroupBy(x => x.UserId)
            .Select(x => new
            {
                Score = x.Sum(ds => ds.Score),
                MessageCount = x.Sum(ds => ds.MessageCount)
            })
            .ToList();

        var medianScore = dailyScoresByUser
            .OrderBy(x => x.Score)
            .Skip((dailyScoresByUser.Count - 1) / 2)
            .Take(2 - dailyScoresByUser.Count % 2)
            .Average(x => x.Score);

        var medianMessageCount = dailyScoresByUser
            .OrderBy(x => x.MessageCount)
            .Skip((dailyScoresByUser.Count - 1) / 2)
            .Take(2 - dailyScoresByUser.Count % 2)
            .Average(x => x.MessageCount);

        var dayGroups = guild.DailyScores.GroupBy(x => x.Date).ToList();

        var averageDayScore = dayGroups.Average(x => x.Sum(ds => ds.Score));
        var averageDayMessage = dayGroups.Average(x => x.Sum(ds => ds.MessageCount));

        var yappiestDayScore = dayGroups.OrderByDescending(x => x.Sum(ds => ds.Score)).First();
        var yappiestDayMessage = dayGroups.OrderByDescending(x => x.Sum(ds => ds.MessageCount)).First();

        var quietestDayScore = dayGroups.OrderBy(x => x.Sum(ds => ds.Score)).First();
        var quietestDayMessage = dayGroups.OrderBy(x => x.Sum(ds => ds.MessageCount)).First();

        var embedBuilder = new EmbedBuilder();

        embedBuilder
            .WithColor(243, 197, 199)
            .WithAuthor("Statistics", Context.Guild.IconUrl)
            .WithTitle(Context.Guild.Name)
            .WithDescription(
                $"- The server median score is **{medianScore:N0}** and the median message count is **{medianMessageCount:N0}**\n"
                + $"- Ranking started on {startDate.ToDiscordTimestamp('D')}");
        
        embedBuilder.AddField("Average", FormatStatString(averageDayScore, averageDayMessage));

        var yappiestScoreTimestamp = yappiestDayScore.Key.ToDiscordTimestamp();
        var yappiestMessageTimestamp = yappiestDayMessage.Key.ToDiscordTimestamp();

        embedBuilder.AddField("Yappiest",
            FormatStatString(yappiestDayScore.Sum(x => x.Score), yappiestDayMessage.Sum(x => x.MessageCount),
                yappiestScoreTimestamp, yappiestMessageTimestamp));

        var quietestScoreTimestamp = quietestDayScore.Key.ToDiscordTimestamp();
        var quietestMessageTimestamp = quietestDayMessage.Key.ToDiscordTimestamp();

        embedBuilder.AddField("Quietest",
            FormatStatString(quietestDayScore.Sum(x => x.Score), quietestDayMessage.Sum(x => x.MessageCount),
                quietestScoreTimestamp, quietestMessageTimestamp));

        embedBuilder.AddField("Totals", FormatStatString(totalScore, totalMessages));

        embedBuilder
            .WithThumbnailUrl("https://cdn.discordapp.com/emojis/1308935597380997151.webp?size=96")
            .WithFooter("Stats valid for when the command was called",
                "https://cdn.discordapp.com/emojis/783328274193448981.webp?size=96")
            .WithCurrentTimestamp();

        var embed = embedBuilder.Build();

        await FollowupAsync(embed: embed, ephemeral: ephemeral);
    }

    #endregion

    #endregion

    #region Handling

    #region Top Handling

    private async Task<(Embed? Embed, MessageComponent? Component)> HandleLeaderboardAsync(int page, OrderType order,
        PeriodDto? period)
    {
        var totalScores = period is null
            ? await _guildRepository.GetAllTotalScoresAsync(Context.Guild.Id)
            : await _guildRepository.GetTotalScoresByPeriodAsync(Context.Guild.Id, period);

        if (order is OrderType.MessageCount)
            totalScores = totalScores
                .OrderByDescending(x => x.MessageCount)
                .ThenByDescending(x => x.Score)
                .ToList();
        else
            totalScores = totalScores
                .OrderByDescending(x => x.Score)
                .ThenByDescending(x => x.MessageCount)
                .ToList();

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
            .WithAuthor(period?.ToString() ?? "All time", Context.Guild.IconUrl)
            .WithTitle($"{Context.Guild.Name} leaderboard")
            .WithDescription($"You're on the **{GetOrdinalString(callerIndex)} place**.");

        embedBuilder.AddField("Position", BuildPositions(scoreboard.Names, page), true);
        embedBuilder.AddField($"{(order is not OrderType.MessageCount ? "\u25bc" : "")} Score",
            BuildScores(scoreboard.Scores), true);
        embedBuilder.AddField($"{(order is OrderType.MessageCount ? "\u25bc" : "")} Messages",
            BuildTotalMessages(scoreboard.MessageCounts), true);

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
        var topRow = new ActionRowBuilder();
        var bottomRow = new ActionRowBuilder();

        if (page is not 0)
            bottomRow
                .WithButton($" ", style: ButtonStyle.Secondary, emote: Emoji.Parse(":arrow_left:"),
                    customId: $"ranking.paging:{page - 1},{order},{period?.Start},{period?.End},{period?.Type}");

        if (page * 10 + 10 < totalScores.Count && totalScores.Count > 10)
            bottomRow
                .WithButton($" ", style: ButtonStyle.Secondary, emote: Emoji.Parse(":arrow_right:"),
                    customId: $"ranking.paging:{page + 1},{order},{period?.Start},{period?.End},{period?.Type}");

        bottomRow.WithButton(order is OrderType.MessageCount ? "Order by score" : "Order by messages",
            style: ButtonStyle.Secondary,
            customId:
            $"ranking.paging:{page},{
                (order is OrderType.MessageCount
                    ? OrderType.Score
                    : OrderType.MessageCount)
            },{period?.Start},{period?.End},{period?.Type}",
            emote: Emoji.Parse(":arrow_down_small:"), disabled: false);

        if (period?.Type is not PeriodType.Specific)
        {
            topRow.WithButton(" ", style: ButtonStyle.Secondary,
                customId: $"ranking.paging:{page},{order},{DateTime.MinValue},{DateTime.MinValue},{PeriodType.Daily}",
                emote: Emoji.Parse(":regional_indicator_d:"), disabled: period?.Type == PeriodType.Daily);

            topRow.WithButton(" ", style: ButtonStyle.Secondary,
                customId: $"ranking.paging:{page},{order},{DateTime.MinValue},{DateTime.MinValue},{PeriodType.Weekly}",
                emote: Emoji.Parse(":regional_indicator_w:"), disabled: period?.Type == PeriodType.Weekly);

            topRow.WithButton(" ", style: ButtonStyle.Secondary,
                customId: $"ranking.paging:{page},{order},{DateTime.MinValue},{DateTime.MinValue},{PeriodType.Monthly}",
                emote: Emoji.Parse(":regional_indicator_m:"), disabled: period?.Type == PeriodType.Monthly);

            topRow.WithButton(" ", style: ButtonStyle.Secondary,
                customId: $"ranking.paging:{page},{order},{DateTime.MinValue},{DateTime.MinValue},{PeriodType.Yearly}",
                emote: Emoji.Parse(":regional_indicator_y:"), disabled: period?.Type == PeriodType.Yearly);

            topRow.WithButton(" ", style: ButtonStyle.Secondary,
                customId: $"ranking.paging:{page},{order},{DateTime.MinValue},{DateTime.MinValue},{PeriodType.AllTime}",
                emote: Emoji.Parse(":regional_indicator_a:"), disabled: period?.Type == PeriodType.AllTime);
        }

        componentBuilder.AddRow(topRow);
        componentBuilder.AddRow(bottomRow);

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

    #region Info Handling

    private string FormatStatString<TNumber>(TNumber scoreStat, TNumber messageStat, 
        string scoreStr = "", string messageStr = "") where TNumber : INumber<TNumber>
    {
        if (!string.IsNullOrWhiteSpace(scoreStr))
            scoreStr = $"({scoreStr})";

        if (!string.IsNullOrWhiteSpace(messageStr))
            messageStr = $"({messageStr})";

        return $"**SCORE\u2002\u2002\u2002\u2002** \u27a1 `{scoreStat:N0}` {scoreStr}\n"
               + $"**MESSAGES** \u27a1 `{messageStat:N0}` {messageStr}";
    }

    #endregion

    #endregion

    #region Types

    public enum OrderType
    {
        Score,
        [ChoiceDisplay("message-count")] MessageCount
    }

    #endregion
}