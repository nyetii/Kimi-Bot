using System.Text.RegularExpressions;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Kimi.Extensions;
using Kimi.Repository.Dtos;
using Kimi.Repository.Repositories;
using Microsoft.EntityFrameworkCore.Storage;
using Serilog;

namespace Kimi.Modules.Ranking;

public class RankingService
{
    private readonly ulong[] _enabledGuilds;
    private readonly string[] _prefix;

    private readonly ILogger<RankingService> _logger;

    private readonly DiscordSocketClient _client;

    private readonly UserRepository _userRepository;

    public RankingService(ILogger<RankingService> logger, IConfiguration config, DiscordSocketClient client,
        UserRepository userRepository)
    {
        _enabledGuilds = config.GetSection("Kimi:Ranking:Guilds").Get<ulong[]>() ?? [];
        _prefix = config.GetSection("Discord:Prefix").Get<string[]>() ?? [];

        _logger = logger;
        _client = client;
        _userRepository = userRepository;
    }

    public async Task InitializeAsync()
    {
        _client.GuildMemberUpdated += OnUserUpdated;
        _client.MessageReceived += OnMessageReceived;
        _client.MessageDeleted += OnMessageDeleted;

        await Task.CompletedTask;
    }

    private async Task OnUserUpdated(Cacheable<SocketGuildUser, ulong> old, SocketGuildUser updated)
    {
        await _userRepository.UpdateNamesAsync(updated);
    }

    private async Task OnMessageReceived(SocketMessage socketMessage)
    {
        if (socketMessage is not SocketUserMessage { Channel: SocketGuildChannel channel } message
            || socketMessage.Author.IsBot)
            return;

        if (_enabledGuilds.All(x => x != channel.Guild.Id))
            return;

        if (message.HasStringPrefix(_prefix) || message.HasMentionPrefix(_client.CurrentUser))
            return;

        var transaction = await _userRepository.BeginTransactionAsync();

        try
        {
            var score = CalculateScore(message);

            _logger.LogDebug("[{user}] {message} - {score}", message.Author.Username, message.CleanContent, score);

            var messageDto = new MessageDto(message);

            await _userRepository.GetOrCreateAsync(messageDto);

            await _userRepository.IncrementScoreAsync(messageDto, score);

            await _userRepository.UpdateLastMessageAsync(messageDto);

            await _userRepository.CommitAsync(transaction);
        }
        catch (Exception ex)
        {
            var netty = await _client.GetUserAsync(191604848423075840);

            if (netty is not null)
            {
                await using var sw = new StreamWriter(new MemoryStream());
                await sw.WriteLineAsync(ex.ToString());
                await sw.FlushAsync();
                await netty.SendFileAsync(sw.BaseStream, "exception.txt", "CATÁSTROFE");
            }

            await transaction.RollbackAsync();
            await transaction.DisposeAsync();

            throw;
        }
    }

    private async Task OnMessageDeleted(Cacheable<IMessage, ulong> cachedMessage,
        Cacheable<IMessageChannel, ulong> cachedChannel)
    {
        if (cachedMessage.Value is not SocketUserMessage { Channel: SocketGuildChannel channel } message
            || cachedMessage.Value.Author.IsBot)
            return;

        if (_enabledGuilds.All(x => x != channel.Guild.Id))
            return;

        if (message.HasStringPrefix(_prefix) || message.HasMentionPrefix(_client.CurrentUser))
            return;

        var transaction = await _userRepository.BeginTransactionAsync();

        try
        {
            var score = CalculateScore(message);

            _logger.LogDebug("[{user}] {message} - {score}", message.Author.Username, message.CleanContent, score);

            var messageDto = new MessageDto(message);

            await _userRepository.DecrementScoreAsync(messageDto, score);

            await _userRepository.CommitAsync(transaction);
        }
        catch (Exception ex)
        {
            var netty = await _client.GetUserAsync(191604848423075840);

            if (netty is not null)
            {
                await using var sw = new StreamWriter(new MemoryStream());
                await sw.WriteLineAsync(ex.ToString());
                await sw.FlushAsync();
                await netty.SendFileAsync(sw.BaseStream, "exception.txt", "CATÁSTROFE");
            }

            await transaction.RollbackAsync();
            await transaction.DisposeAsync();

            throw;
        }
    }

    private uint CalculateScore(SocketUserMessage message)
    {
        if (Regexes.UrlMatch().IsMatch(message.CleanContent))
        {
            var regex = Regexes
                .UrlMatch()
                .Split(message.CleanContent)
                .Where(str => !string.IsNullOrWhiteSpace(str))
                .ToArray();

            var log = Math.Log2(regex.Length is 1 ? 2 : regex.Length);

            return (uint)Math.Clamp(log, 0, 3);
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
            return attachmentsFactor > 4
                ? (uint)(message.Attachments.Count - Math.Log10(message.Attachments.Count) *
                    (message.Attachments.Count / 2.0))
                : (uint)attachmentsFactor;

        return (uint)(message.CleanContent.Length > 15
            ? (uint)Math.Log2(message.CleanContent.Length) + attachmentsFactor
            : message.CleanContent.Length / 2 + attachmentsFactor);
    }

    private uint CalculateTextQualityScore(List<Match> matches, SocketUserMessage message)
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

        return (uint)Math.Clamp(messageLength + attachmentsFactor - factor, 0, messageLength);
    }
}