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
}