using System.Text.RegularExpressions;

namespace Kimi.Modules.Ranking;

public static partial class Regexes
{
    [GeneratedRegex("(https?:\\/\\/(?:www\\.|(?!www))[a-zA-Z0-9][a-zA-Z0-9-]+[a-zA-Z0-9]\\.[^\\s]{2,}|www\\.[a-zA-Z0-9][a-zA-Z0-9-]+[a-zA-Z0-9]\\.[^\\s]{2,}|https?:\\/\\/(?:www\\.|(?!www))[a-zA-Z0-9]+\\.[^\\s]{2,}|www\\.[a-zA-Z0-9]+\\.[^\\s]{2,})", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
    public static partial Regex UrlMatch();

    [GeneratedRegex(@"(.)\1{2,}", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
    public static partial Regex RepetitionMatch();

    [GeneratedRegex(@"<(?::\w+:|!@*&*|#)[0-9]+>", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
    public static partial Regex EmoteMatch();
}