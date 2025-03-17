using System.Text;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace Kimi.Extensions;

public static class DiscordExtensions
{
    public static string ParseHandle(this string handle) => handle[2..].TrimEnd('>');
    public static bool TryParseHandle(this string handle, out string id)
    {
        if(handle.StartsWith("<@"))
        {
            id = handle.ParseHandle();
            return true;
        }

        id = handle;
        return false;
    }
    
    public static bool HasStringPrefix(this SocketUserMessage message, IEnumerable<string> prefixes)
    {
        var pos = 0;
        return prefixes.Any(prefix => message.HasStringPrefix(prefix, ref pos));
    }

    public static bool HasMentionPrefix(this SocketUserMessage message, IUser user)
    {
        var pos = 0;
        return message.HasMentionPrefix(user, ref pos);
    }

    public static async Task SendToLogChannelAsync(this DiscordSocketClient client, ulong channelId, string message)
    {
        if (channelId is 0)
            return;
        
        var channel = client.GetChannel(channelId);

        if (channel is ISocketMessageChannel socketChannel)
        {
            await socketChannel.SendMessageAsync(message);
        }
    }

    public static async Task SendToLogChannelAsync(this DiscordSocketClient client, ulong channelId, Exception exception)
    {
        if (channelId is 0)
            return;
        
        var strBuilder = new StringBuilder();
        strBuilder.AppendLine("```yaml")
            .AppendLine($"Method: {exception.TargetSite?.ToString()}")
            .AppendLine($"{exception.GetType().Name}: {exception.Message}")
            .AppendLine()
            .AppendLine("!STACKTRACE")
            .AppendLine(exception.StackTrace)
            .AppendLine("```");
        
        await client.SendToLogChannelAsync(channelId, strBuilder.ToString());
    }

    public static long ToUnixTimeSeconds(this DateOnly date)
    {
        return new DateTimeOffset(date, new TimeOnly(), TimeSpan.Zero).ToUnixTimeSeconds();
    }

    public static string ToDiscordTimestamp(this DateOnly date, char type = 'R')
    {
        return $"<t:{date.ToUnixTimeSeconds()}:{type}>";
    }
}