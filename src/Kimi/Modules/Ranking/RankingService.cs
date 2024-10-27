using System.Text.RegularExpressions;
using Discord;
using Discord.Rest;
using Discord.WebSocket;

namespace Kimi.Modules.Ranking;

public class RankingService
{
    private readonly ulong[] _enabledGuilds;

    private readonly ILogger<RankingService> _logger;

    private readonly DiscordSocketClient _client;

    public RankingService(ILogger<RankingService> logger, IConfiguration config, DiscordSocketClient client)
    {
        _enabledGuilds = config.GetSection("Kimi:Ranking:Guilds").Get<ulong[]>() ?? [];

        _logger = logger;
        _client = client;
    }

    public async Task InitializeAsync()
    {
        _client.MessageReceived += OnMessageReceived;
        await Task.CompletedTask;
    }

    private async Task OnMessageReceived(SocketMessage socketMessage)
    {
        if (socketMessage is not SocketUserMessage { Channel: SocketGuildChannel channel } message || socketMessage.Author.IsBot)
            return;

        if (_enabledGuilds.All(x => x != channel.Guild.Id))
            return;

        var score = CalculateScore(message);

        _logger.LogDebug("[{user}] {message} - {score}", message.Author.Username, message.CleanContent, score);

        await Task.CompletedTask;
    }

    private int CalculateScore(SocketUserMessage message)
    {
        if (Regexes.UrlMatch().IsMatch(message.CleanContent))
        {
            var regex = Regexes
                .UrlMatch()
                .Split(message.CleanContent)
                .Where(str => !string.IsNullOrWhiteSpace(str))
                .ToArray();

            var log = Math.Log2(regex.Length is 1 ? 2 : regex.Length);

            return (int)Math.Clamp(log, 0, 3);
        }

        if ((message.Flags & MessageFlags.VoiceMessage) != 0)
            return 10;

        if (Regexes.RepetitionMatch().IsMatch(message.CleanContent))
        {
            var matches = Regexes
                .RepetitionMatch()
                .Matches(message.CleanContent)
                .ToList();

            return CalculateTextQualityScore(matches, message);
        }

        if (Regexes.EmoteMatch().IsMatch(message.Content))
        {
            var matches = Regexes
                .EmoteMatch()
                .Matches(message.Content)
                .ToList();

            return CalculateTextQualityScore(matches, message);
        }

        var attachmentsFactor = message.Attachments.Count is > 0 and <= 4
            ? 2 + message.Attachments.Count / 2
            : 0;

        if (string.IsNullOrWhiteSpace(message.CleanContent) && message.Attachments.Any(x =>
                x.ContentType.Contains("image/") || x.ContentType.Contains("video/")))
            return attachmentsFactor == 0 ? (int)(message.Attachments.Count - Math.Log10(message.Attachments.Count) * 2) : attachmentsFactor;

        return message.CleanContent.Length > 15
            ? (int)Math.Log2(message.CleanContent.Length) + attachmentsFactor
            : message.CleanContent.Length / 2 + attachmentsFactor;
    }

    private int CalculateTextQualityScore(List<Match> matches, SocketUserMessage message)
    {
        var messageLength = (double)message.CleanContent.Length;

        if (messageLength > 15)
            messageLength = Math.Log2(messageLength);
        else
            messageLength /= 2;

        var factor = matches
            .Select(x => x.Value.Length)
            .Max(count => count / messageLength);

        var attachmentsFactor = message.Attachments.Count is > 0 and <= 4
            ? 2 + message.Attachments.Count / 2
            : 0;

        return (int)Math.Clamp(messageLength + attachmentsFactor - factor, 0, messageLength);
    }
}