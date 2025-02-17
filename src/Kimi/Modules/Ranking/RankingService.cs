using System.Text.RegularExpressions;
using Discord;
using Discord.WebSocket;
using Kimi.Repository.Dtos;
using Kimi.Repository.Repositories;

namespace Kimi.Modules.Ranking;

public class RankingService
{
    private readonly ulong[] _enabledGuilds;

    private readonly ILogger<RankingService> _logger;

    private readonly DiscordSocketClient _client;
    
    private readonly UserRepository _userRepository;

    public RankingService(ILogger<RankingService> logger, IConfiguration config, DiscordSocketClient client, UserRepository userRepository)
    {
        _enabledGuilds = config.GetSection("Kimi:Ranking:Guilds").Get<ulong[]>() ?? [];

        _logger = logger;
        _client = client;
        _userRepository = userRepository;
    }

    public async Task InitializeAsync()
    {
        _client.MessageReceived += OnMessageReceived;

        // Task
        //     .Run(async () =>
        //     {
        //         var timer = new PeriodicTimer(TimeSpan.FromMinutes(10));
        //
        //         while (await timer.WaitForNextTickAsync())
        //         {
        //             await _dbContext.SaveChangesAsync();
        //             _logger.LogDebug("Saved score.");
        //         }
        //     })
        //     .ContinueWith(task =>
        //     {
        //         if (task is not { IsFaulted: true, Exception: not null })
        //             return;
        //
        //         _logger.LogCritical(task.Exception, "Could not save score.");
        //         throw task.Exception;
        //     });

        await Task.CompletedTask;
    }

    private async Task OnMessageReceived(SocketMessage socketMessage)
    {
        if (socketMessage is not SocketUserMessage { Channel: SocketGuildChannel channel } message || socketMessage.Author.IsBot)
            return;

        if (_enabledGuilds.All(x => x != channel.Guild.Id))
            return;

        try
        {
            var score = CalculateScore(message);

            _logger.LogDebug("[{user}] {message} - {score}", message.Author.Username, message.CleanContent, score);

            var messageDto = new MessageDto(message);

            var transaction = await _userRepository.BeginTransactionAsync();
            
            var user = await _userRepository.GetOrCreateAsync(messageDto);

            await _userRepository.IncrementScoreAsync(messageDto, score);
            
            await _userRepository.UpdateLastMessageAsync(messageDto);

            await _userRepository.CommitAsync(transaction);

            // var daily = await _dbContext.DailyScores
            //     .Include(x => x.Guild)
            //     .Include(x => x.User)
            //     .ThenInclude(x => x.GuildsUsers)
            //     .Where(x => x.UserId == socketMessage.Author.Id
            //                 && x.GuildId == channel.Guild.Id)
            //     .FirstOrDefaultAsync(x => x.Date == DateOnly.FromDateTime(DateTime.Now));
            //
            // if (daily is null)
            // {
            //     daily = new DailyScore
            //     {
            //         Date = DateOnly.FromDateTime(DateTime.Now),
            //         GuildId = channel.Guild.Id,
            //         UserId = message.Author.Id,
            //         Score = (uint)score
            //     };
            //
            //     var user = await _dbContext.Users.FirstOrDefaultAsync(x => x.Id == message.Author.Id);
            //
            //     if (user is null)
            //     {
            //         user = new User
            //         {
            //             Id = message.Author.Id,
            //             Username = message.Author.Username,
            //             Guilds = [],
            //             DailyScores = [],
            //         };
            //
            //         var nickname = channel.Guild.Users.FirstOrDefault(x => x.Id == message.Author.Id)?.Nickname;
            //
            //         var guild = await _dbContext.Guilds.FirstAsync(x => x.Id == channel.Guild.Id);
            //         user.Guilds.Add(guild);
            //         user.GuildsUsers.Add(new GuildUser
            //         {
            //             GuildId = guild.Id,
            //             UserId = user.Id,
            //             Nickname = nickname
            //         });
            //
            //         await _dbContext.Users.AddAsync(user);
            //     }
            //     daily.User = user;
            //
            //     daily.User.DailyScores.Add(daily);
            //
            //     await _dbContext.DailyScores.AddAsync(daily);
            // }
            // else
            // {
            //     daily.Score += (uint)score;
            //     _dbContext.DailyScores.Update(daily);
            // }
            //
            // var userInfo = daily.User.GetGuildUserInfo(channel.Guild.Id);
            // userInfo.LastMessage = message.Timestamp.DateTime;
            //
            // _dbContext.GuildUsers.Update(userInfo);
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
            return attachmentsFactor == 0 ? (uint)(message.Attachments.Count - Math.Log10(message.Attachments.Count) * 2) : 0;

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