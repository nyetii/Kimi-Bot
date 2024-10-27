using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace Kimi.Commands;

public class Test : ModuleBase<SocketCommandContext>
{
    private readonly ILogger<Test> _logger;

    private readonly DiscordSocketClient _client;

    public Test(ILogger<Test> logger, DiscordSocketClient client)
    {
        _logger = logger;
        _client = client;
    }

    [Command("read")]
    public async Task HandleTestCommand()
    {

        _logger.LogInformation("{guilds}", string.Join(", ", _client.Guilds.Select(x => x.Name)));

        var ponyville = _client.Guilds.First(x => x.Name is "ponyville");

        var channel = ponyville.GetTextChannel(ponyville.Channels.First(x => x.Name is "mush-room").Id);

        var messages = await channel.GetMessagesAsync().FlattenAsync();

        foreach (var message in messages)
        {
            _logger.LogInformation("[{user}] {message}", message.Author, message.CleanContent);
        }

        //foreach (var guild in _client.Guilds.Where(x => x.Name is "ponyville"))
        //{
        //    _logger.LogInformation("Users from {guild}: {users}", guild.Name, string.Join(", ", guild.Users.Select(x => x.Username)));
        //    _logger.LogInformation("Channels from {guild}: {channels}", guild.Name, string.Join(", ", guild.Channels.Select(x => x.Name)));
        //}

        await Context.Message.ReplyAsync("it works!");
    }

    [Command("talk")]
    public async Task HandleTalkCommand([Remainder] string text)
    {
        if (Context.User.Username is not "netty")
        {
            await Context.Message.ReplyAsync("only nettle can talk to them sillyy");
            return;
        }

        var ponyville = _client.Guilds.First(x => x.Name is "ponyville");

        var channel = ponyville.GetTextChannel(ponyville.Channels.First(x => x.Name is "mush-room").Id);

        var message = await channel.SendMessageAsync(text);

        await Context.Message.ReplyAsync($"The id of the message sent is {message.Id}");
    }
}