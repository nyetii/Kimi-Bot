using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Kimi.GPT2
{
    internal class Generation
    {
        private static string? _content;
        internal static async Task<string> Parse(string content, string? seed)
        {
            _content = content;
            await FixEncoding();
            if (string.IsNullOrWhiteSpace(seed))
                _content = TrimContent();

            return _content;
        }

        private static async Task FixEncoding()
        {
            _content = Regex.Replace(_content, "Ú", "é");
            _content = Regex.Replace(_content, "Ò", "ã");
            _content = Regex.Replace(_content, "þ", "ç");
            _content = Regex.Replace(_content, "Ý", "í");
            _content = Regex.Replace(_content, "Û", "ê");
            _content = Regex.Replace(_content, "ß", "á");
            _content = Regex.Replace(_content, "¾", "ó");
            _content = Regex.Replace(_content, "·", "ú");
            _content = Regex.Replace(_content, "Ô", "â");
            _content = Regex.Replace(_content, "╔", "É");

            await Task.CompletedTask;
        }

        private static string TrimContent()
        {
            string[] splitContent = _content.Split(',', 2);

            return splitContent[1].TrimStart();
        }
    }
}
