using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.Interactions;
using Kimi.Services.Core;
using LibGit2Sharp;
using RequireOwner = Discord.Commands.RequireOwnerAttribute;

namespace Kimi.Commands.Modules.Utils
{
    public class CommandInfo : ModuleBase<SocketCommandContext>
    {
        private readonly InteractionService? _slash;
        public CommandInfo(InteractionService? slash = null)
        {
            _slash = slash;
        }
        
        [Command("slashCommandsRegistered")]
        [Alias("sc", "scr")]
        public async Task SlashCommandsRegistered()
        {
            await ReplyAsync($"```fix\n{Info.SlashCommandsTable}\n```");
        }

        public async Task<string> HandleSlashCommandsTable()
        {
            string response = "Slash Commands registered\n";

            string[] scModule = new string[_slash.SlashCommands.Count];
            string[] scName = new string[_slash.SlashCommands.Count];
            string[] scMethod = new string[_slash.SlashCommands.Count];

            for (int i = 0; i < _slash.SlashCommands.Count; i++)
            {
                scModule[i] = _slash.SlashCommands.ElementAt(i).Module.Name;
                scName[i] = _slash.SlashCommands.ElementAt(i).Name;
                scMethod[i] = _slash.SlashCommands.ElementAt(i).MethodName;
            }

            response += $"\n     {"Module",(40 - 6) / 2 - 6} {"Name",(40 - 4) / 2 + 4} {"Method",(40 + 3) / 2}";

            for (int i = 0; i < scName.Length; i++)
            {
                response += $"\n     {scModule[i],-20} {scName[i],-20} {scMethod[i],-20}{(i == scName.Length - 1 ? "\n" : "")}";
            }

            Info.SlashCommandsTable = response;

            return await Task.FromResult(Info.SlashCommandsTable);
        }
    }
}
