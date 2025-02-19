using Discord.Interactions;

namespace Kimi.Commands.Birthday;

[DontAutoRegister]
[Group("birthday", "Birthday!!!")]
public class BirthdayModule : InteractionModuleBase<SocketInteractionContext>
{
    [SlashCommand("test", "test command")]
    public async Task Test() => await RespondAsync("happy birthday!!!!");
}