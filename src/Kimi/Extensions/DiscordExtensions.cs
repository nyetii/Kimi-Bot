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
}