using System.Text.RegularExpressions;
using Discord;
using Discord.WebSocket;
using Kimi.Configuration;
using Kimi.Extensions;
using Kimi.Repository.Dtos;
using Kimi.Repository.Models;
using Kimi.Repository.Repositories;
using Microsoft.Extensions.Options;

namespace Kimi.Modules.Ranking;

public class RankingService
{
    private readonly KimiConfiguration _configuration;
    private readonly ILogger<RankingService> _logger;

    private readonly DiscordSocketClient _client;

    private readonly LevelService _levelService;
    private readonly UserRepository _userRepository;

    public RankingService(ILogger<RankingService> logger, IOptions<KimiConfiguration> options,
        DiscordSocketClient client,
        UserRepository userRepository, LevelService levelService)
    {
        _configuration = options.Value;

        _logger = logger;
        _client = client;
        _userRepository = userRepository;
        _levelService = levelService;
    }

    public async Task InitializeAsync()
    {
        _logger.LogInformation("Initializing Ranking Service");
        _client.GuildMemberUpdated += OnUserUpdated;
        _client.MessageReceived += OnMessageReceived;
        _client.MessageDeleted += OnMessageDeleted;

        await _levelService.InitializeAsync();
    }

    private async Task OnUserUpdated(Cacheable<SocketGuildUser, ulong> old, SocketGuildUser updated)
    {
        try
        {
            await _userRepository.UpdateNamesAsync(updated);
        }
        catch (Exception ex)
        {
            await _client.SendToLogChannelAsync(_configuration.LogChannel, ex);
            throw;
        }
    }

    private async Task OnMessageReceived(SocketMessage socketMessage)
    {
        if (socketMessage is not SocketUserMessage { Channel: SocketGuildChannel channel } message
            || socketMessage.Author.IsBot)
            return;

        if (_configuration.Guilds.All(x => x.Id != channel.Guild.Id))
            return;

        if (message.HasStringPrefix(_configuration.Prefixes) || message.HasMentionPrefix(_client.CurrentUser))
            return;

        var transaction = await _userRepository.BeginTransactionAsync();

        try
        {
            var score = CalculateScore(message);

            var messageDto = new MessageDto(message);
            _logger.LogDebug("Score before deduction {score}", score);
            var user = await _userRepository.GetOrCreateAsync(messageDto);

            var guildUser = user.GetGuildUserInfo(channel.Guild.Id);
            score = RetractTextSpamScore(guildUser, messageDto, score);

            _logger.LogDebug("[{user}] {message} - {score}", message.Author.Username, message.CleanContent, score);

            await _userRepository.IncrementScoreAsync(messageDto, score);

            await _userRepository.UpdateLastMessageAsync(messageDto);

            await _userRepository.CommitAsync(transaction);
        }
        catch (Exception ex)
        {
            await _client.SendToLogChannelAsync(_configuration.LogChannel, ex);

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

        if (_configuration.Guilds.All(x => x.Id != channel.Guild.Id))
            return;

        if (message.HasStringPrefix(_configuration.Prefixes) || message.HasMentionPrefix(_client.CurrentUser))
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
            await _client.SendToLogChannelAsync(_configuration.LogChannel, ex);

            await transaction.RollbackAsync();
            await transaction.DisposeAsync();

            throw;
        }
    }

    private uint CalculateScore(SocketUserMessage message)
    {
        if (RankingRegex.UrlMatch().IsMatch(message.CleanContent))
        {
            var regex = RankingRegex
                .UrlMatch()
                .Split(message.CleanContent)
                .Where(str => !string.IsNullOrWhiteSpace(str))
                .ToArray();

            var log = Math.Log2(regex.Length is 1 ? 2 : regex.Length);

            return (uint)Math.Clamp(log, 0, 3);
        }

        if ((message.Flags & MessageFlags.VoiceMessage) != 0)
            return 10;

        if (RankingRegex.RepetitionMatch().IsMatch(message.CleanContent))
        {
            var matches = RankingRegex
                .RepetitionMatch()
                .Matches(message.CleanContent)
                .ToList();

            return CalculateTextQualityScore(matches, message);
        }

        if (RankingRegex.EmoteMatch().IsMatch(message.Content))
        {
            var matches = RankingRegex
                .EmoteMatch()
                .Matches(message.Content)
                .ToList();

            return CalculateTextQualityScore(matches, message);
        }

        var attachmentsFactor = message.Attachments.Count is > 0 and <= 4
            ? 2 + message.Attachments.Count / 2
            : 0;

        if (string.IsNullOrWhiteSpace(message.CleanContent) && message.Attachments.Any(x
                => !string.IsNullOrWhiteSpace(x.ContentType)
                   && (x.ContentType.Contains("image/") || x.ContentType.Contains("video/"))))
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

    private uint RetractTextSpamScore(GuildUser user, MessageDto message, uint score)
    {
        if (user.LastMessage is null)
            return score;

        return message.DateTime.Subtract(user.LastMessage.Value).Seconds switch
        {
            < 2 => 0,
            < 5 => score / 4,
            < 10 => score / 3,
            < 15 => score / 2,
            _ => score
        };
    }
}