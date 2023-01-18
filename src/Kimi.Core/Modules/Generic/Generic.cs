﻿using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kimi.Core.Services;

namespace Kimi.Core.Modules.Generic
{
    public class Generic : ModuleBase<SocketCommandContext>
    {
        [Command("monoana")]
        [Summary("Primeiro comando feito para o bot")]
        public async Task HandlePingCommand()
        {
            await Context.Message.ReplyAsync("https://media.discordapp.net/attachments/896243750228090974/952247379732619326/lobber.gif");
        }

        [Command("info")]
        public async Task HandleHelpCommand()
        {
            var embed = new EmbedBuilder()
                .WithAuthor("Hello there, I'm Kimi!",
                    Context.Client.CurrentUser.GetAvatarUrl())
                .WithDescription("Sorry, I am still yet to get better!")
                .WithFooter("Version " + Info.Version)
                .WithColor(241, 195, 199)
                .Build();

            await ReplyAsync(embed: embed);
        }

        [Command("ping")]
        public async Task HandleTruePingCommand()
        {
            await ReplyAsync($"{Context.Client.Latency}ms <:pacemike:841313592179163166>");
        }
    }
}
