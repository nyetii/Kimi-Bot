using Discord.Interactions;

namespace Kimi.Commands;

public class SlashTest : InteractionModuleBase<SocketInteractionContext>
{
    private readonly ILogger<SlashTest> _logger;

    public SlashTest(ILogger<SlashTest> logger)
    {
        _logger = logger;
    }

    [SlashCommand("slash-test", "Slash test command")]
    public async Task SlashTestCommand()
    {
        await RespondAsync("it works!");
    }
}