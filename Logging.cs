using Discord;
using Discord.Commands;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kimi
{
    class Logging
    {
        public static async Task PermissionDeniedAsync(string command, IUser user)
        {
            Log.Information("Permission denied! - Command <{command}> called by {user}#{tag}", command, user.Username, user.Discriminator);
            await Task.CompletedTask;
        }

        public static async Task PermissionAllowedAsync(string command, IUser user)
        {
            Log.Information("Got it! - Command <{command}> called by {user}#{tag}", command, user.Username, user.Discriminator);
            await Task.CompletedTask;
        }

        public static async Task ExceptionAsync(Exception ex, string command, IUser user)
        {
            Log.Error("Exception caught... - Command <{command}> called by {user}#{tag}\n{ex}", command, user.Username, user.Discriminator, ex);
            await Task.CompletedTask;
        }

        public static async Task ExceptionCodeAsync(Exception ex)
        {
            Log.Error("Exception caught... - {ex}", ex);
            await Task.CompletedTask;
        }
    }

    class Errors
    {
        public static async Task<Embed> ExceptionAsync(string exception)
        {
            var description = $"Exception caught...```csharp\n{exception}\n```";
            return await ErrorEmbedAsync(description);
        }

        public static async Task<Embed> ParameterAsync(string exception)
        {
            var description = $"There's not such a parameter as `{exception}`.";
            return await ErrorEmbedAsync(description);
        }

        public static async Task<Embed> ErrorEmbedAsync(string description)
        {
            var embed = new EmbedBuilder();


            embed
                .WithAuthor("Error!\n", "https://cdn.discordapp.com/attachments/973407684579688558/1037246401404473344/783328274193448981.png")
                .WithDescription(description)
                .WithColor(241, 195, 199)
                .Build();

            return await Task.FromResult(embed.Build());
        }
    }
}
