using Discord;
using Discord.Commands;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kimi.Modules
{
    public class State : ModuleBase<SocketCommandContext>
    {

        [Command("shutdown")]
        public async Task ShutdownAsync()
        {
            var content = Context.Message.CleanContent;
            IUser user = Context.Message.Author;
            try
            {
                string jsonString = File.ReadAllText($"{Environment.CurrentDirectory}/appconfig.json");
                var json = JObject.Parse(jsonString);
                if (user.Id.Equals(ulong.Parse(json["AdminID"].ToString())))
                {
                    await ReplyAsync("Shutting down!", allowedMentions: Discord.AllowedMentions.None);
                    await Logging.PermissionAllowedAsync(content, user);
                    //Log.Information("Shutting down! - Command called by {user}#{tag}", user.Username, user.Discriminator);
                    Environment.Exit(0);
                }
                else 
                {
                    
                    await ReplyAsync("<:Kryabeans2:883406368118276106>", allowedMentions: Discord.AllowedMentions.None);
                    await Logging.PermissionDeniedAsync(content, user);
                    //Log.Information("!shutdown denied - Command called by {user}#{tag}", user.Username, user.Discriminator);
                }
            }
            catch(Exception ex)
            {
                await ReplyAsync(embed: await Errors.ExceptionAsync(ex.ToString()));
                await Logging.ExceptionAsync(ex, content, user);
            }
        }

        [Command("restart")]
        public async Task RestartAsync(params string[] console)
        {
            var content = Context.Message.CleanContent;
            IUser user = Context.Message.Author;
            try
            {
                string jsonString = File.ReadAllText($"{Environment.CurrentDirectory}/appconfig.json");
                var json = JObject.Parse(jsonString);
                if (user.Id.Equals(ulong.Parse(json["AdminID"].ToString())))
                {
                    if (console.Length == 1 && console[0].Equals("console"))
                    {
                        await ReplyAsync("I'll be right back!", allowedMentions: Discord.AllowedMentions.None);
                        await Logging.PermissionAllowedAsync(content, user);
                        Process cmd = new Process();
                        cmd.StartInfo.FileName = $"{Environment.CurrentDirectory}/Kimi.exe";
                        cmd.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
                        cmd.StartInfo.CreateNoWindow = false;
                        cmd.StartInfo.UseShellExecute = true;

                        cmd.Start();
                        Environment.Exit(0);
                    }
                    else if(console.Length < 1)
                    {
                        await ReplyAsync("I'll be right back!", allowedMentions: Discord.AllowedMentions.None);
                        await Logging.PermissionAllowedAsync(content, user);
                        //Log.Information("Shutting down! - Command called by {user}#{tag}", user.Username, user.Discriminator);
                        Process cmd = new Process();
                        cmd.StartInfo.FileName = $"{Environment.CurrentDirectory}/Kimi.exe";
                        cmd.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                        cmd.StartInfo.CreateNoWindow = true;
                        cmd.StartInfo.UseShellExecute = false;

                        cmd.Start();
                        Environment.Exit(0);
                    }
                    else
                    {
                        string parameter = null;
                        for(int i = 0; i < console.Length; i++)
                        {
                            parameter = parameter + " " + console[i];
                        }
                        await ReplyAsync(embed: await Errors.ParameterAsync(parameter.TrimStart(' ')), allowedMentions: Discord.AllowedMentions.None);
                    }
                }
                else
                {

                    await ReplyAsync("<:Kryabeans2:883406368118276106>", allowedMentions: Discord.AllowedMentions.None);
                    await Logging.PermissionDeniedAsync(content, user);
                    //Log.Information("!shutdown denied - Command called by {user}#{tag}", user.Username, user.Discriminator);
                }
            }
            catch (Exception ex)
            {
                await ReplyAsync(embed: await Errors.ExceptionAsync(ex.ToString()));
                await Logging.ExceptionAsync(ex, content, user);
            }
        }
    }
}
