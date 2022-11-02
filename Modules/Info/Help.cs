using Discord;
using Discord.Commands;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kimi.Modules.Info
{
    [Group("help")]
    [Alias("socorro")]
    class Help : ModuleBase<SocketCommandContext>
    {
        private readonly CommandService _commands;
        public Help(CommandService commands)
        {
            _commands = commands;
        }

        // Commands

        //[Command(null)]
        //public async Task HelpAsync()
        //{
        //    await ReplyAsync(embed: await HelpEmbedBuildAsync());
        //}

        [Command(null)]
        public async Task SpecificHelpAsync(params string[] command)
        {
            try
            {
                if (command.Length > 0)
                {


                    await ReplyAsync(embed: await SpecificHelpEmbedBuildAsync(command));
                }
                else
                {
                    await ReplyAsync(embed: await HelpEmbedBuildAsync());
                }
            } catch(Exception ex) { await ReplyAsync(ex.ToString()); }
        }

        public async Task<Embed> HelpEmbedBuildAsync()
        {
            List<CommandInfo> commands = _commands.Commands.ToList();
            var embed = new EmbedBuilder();
            foreach (var module in _commands.Modules)
            {
                string description = null;
                foreach (var command in module.Commands)
                {
                    var result = await command.CheckPreconditionsAsync(Context);
                    if (result.IsSuccess)
                        description += $"`!{command.Aliases.First()}`\n";
                }

                if (!string.IsNullOrWhiteSpace(description))
                {
                    embed.AddField(x =>
                    {
                        x.Name = module.Name.ToUpperInvariant();
                        x.Value = description;
                        x.IsInline = true;
                    });
                }
            }


            embed
                .WithAuthor("Kimi comes in clutch!\n", "https://cdn.discordapp.com/attachments/973407684579688558/1037246401404473344/783328274193448981.png")
                .WithDescription("For more specific information, type `!help <command>`\nFurther questions? Ping `nyetii#0725` <:vietcat:888614909632462868>")
                .WithColor(241, 195, 199)
                .Build();

            return await Task.FromResult(embed.Build());
        }

        public async Task<Embed> SpecificHelpEmbedBuildAsync(string[] cmd)
        {
            var embed = new EmbedBuilder();
            string alias = null;
            string description = null;
            bool foundCommand = false;
            foreach (var command in _commands.Commands)
            {
                for (int i = 0; i < cmd.Length && !foundCommand; i++)
                {
                    if (command.Name.Equals(cmd[i]) && !foundCommand)
                    {
                        foundCommand = true;
                        alias = command.Module.Group != null ? $"Help - !{command.Module.Group} {command.Name}" : $"Help - !{command.Name}";
                        string name = null;
                        string fieldValue = null;
                        try
                        {
                            var result = await command.CheckPreconditionsAsync(Context);
                            if (result.IsSuccess && command.HasVarArgs)
                            {
                                name += $"{command.Parameters.FirstOrDefault().Name}\n";
                                fieldValue += $"{command.Parameters.FirstOrDefault().Summary}\n";
                            }
                            else if (result.IsSuccess)
                            {
                                description += command.Summary != null ? $"{command.Summary}\n" : $"Sem palavras.\n";
                            }

                            if (!string.IsNullOrWhiteSpace(fieldValue))
                            {
                                embed.AddField(x =>
                                {
                                    x.Name = name;
                                    x.Value = fieldValue;
                                    x.IsInline = true;
                                });
                            }
                        }
                        catch (Exception ex)
                        {
                            embed.AddField(x =>
                            {
                                x.Name = ex.ToString();
                                x.Value = ex.ToString();
                                x.IsInline = true;
                            });
                        }
                    }


                }

            }
            if (!foundCommand)
            {
                foreach (var command in _commands.Commands)
                {
                    if (command.Module.Name.Equals(cmd[0]) && !foundCommand)
                    {
                        string jsonString = File.ReadAllText($"{Environment.CurrentDirectory}/Modules/Info/description.json");
                        var summary = JObject.Parse(jsonString);
                        foundCommand = true;
                        alias = $"Help - !{cmd[0]}";
                        description = summary[cmd[0]].ToString();
                    }
                }
            }



            embed
                .WithAuthor(alias != null ? alias : "ERRO",
                alias != null ? "https://cdn.discordapp.com/attachments/973407684579688558/1037247079694749786/857141466635042837.png" :
                "https://cdn.discordapp.com/attachments/973407684579688558/1037247435854061578/949402932149903430.png")
                .WithDescription(description != null ? description : "Comando não foi encontrado.")
                .WithColor(241, 195, 199)
                .Build();

            return await Task.FromResult(embed.Build());
        }
    }
}
