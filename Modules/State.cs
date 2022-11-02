using Discord;
using Discord.Commands;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
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
            try
            {
                string jsonString = File.ReadAllText($"{Environment.CurrentDirectory}/appconfig.json");
                var json = JObject.Parse(jsonString);
                IUser user = Context.Message.Author;
                if (user.Id.Equals(ulong.Parse(json["AdminID"].ToString())))
                {
                    await ReplyAsync("Shutting down!", allowedMentions: Discord.AllowedMentions.None);
                    Environment.Exit(0);
                }
                else { await ReplyAsync("<:Kryabeans2:883406368118276106>", allowedMentions: Discord.AllowedMentions.None); }
            }
            catch(Exception ex)
            {
                await ReplyAsync(ex.ToString());
            }
        }
    }
}
