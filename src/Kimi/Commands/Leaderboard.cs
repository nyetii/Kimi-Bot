using Discord.Interactions;
using Kimi.Repository.Repositories;

namespace Kimi.Commands;

public class Leaderboard : InteractionModuleBase<SocketInteractionContext>
{
    private readonly ILogger<Leaderboard> _logger;
    private readonly GuildRepository _guildRepository;

    public Leaderboard(ILogger<Leaderboard> logger, IServiceProvider provider)
    {
        _logger = logger;
        _guildRepository = provider.CreateScope().ServiceProvider.GetRequiredService<GuildRepository>();
    }
    
    [SlashCommand("leaderboard", "Test command.")]
    public async Task ListLeaderboard()
    {
        var scores = (await _guildRepository.GetAllTotalScoresAsync(Context.Guild.Id)).Take(10);

        var str = "";
        foreach (var score in scores)
        {
            str += $"1. {score.Nickname} - {score.Score}\n";
        }
        
        await RespondAsync($"TOP 10 scores for {Context.Guild.Name}\n{str}");
    }
}