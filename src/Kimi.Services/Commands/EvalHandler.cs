using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;

namespace Kimi.Services.Commands
{
    public class EvalHandler
    {
        private readonly DiscordSocketClient _client;
        private readonly SocketCommandContext _context;

        public EvalHandler(DiscordSocketClient client, SocketCommandContext context)
        {
            _client = client;
            _context = context;
        }

        public async Task EvalAsync(string code)
        {
            var typingState = _context.Channel.EnterTypingState();
            var msg = await _context.Message.ReplyAsync(embed: EmbedHelper.Build("Evaluating...",
                type: EmbedHelper.Type.Waiting));
            try
            {
                var result = await EvaluateCodeAsync(code);

                if (result is { ReturnValue: { } } && !string.IsNullOrWhiteSpace(result.ReturnValue.ToString()))
                {
                    await msg.ModifyAsync(x =>
                        x.Embed = EmbedHelper.Build("Evaluation Result", result.ReturnValue.ToString(),
                            EmbedHelper.Type.Success))!;
                }
                else
                {
                    await msg.ModifyAsync(x =>
                        x.Embed = EmbedHelper.Build("Evaluation Successful", "No result was returned.",
                            EmbedHelper.Type.Success))!;
                }
            }
            catch (Exception ex)
            {
                _client.ButtonExecuted += async (component) =>
                {
                    if (component.User.Id != _context.User.Id)
                    {
                        await component.RespondAsync(
                            embed: EmbedHelper.Build("Error", "This button is not for you!",
                                EmbedHelper.Type.Failure), ephemeral: true);
                        return;
                    }

                    if (component.Message.Id != msg.Id)
                    {
                        await component.RespondAsync(
                            embed: EmbedHelper.Build("Error", "Can only unwind the stack for the last eval command executed",
                                EmbedHelper.Type.Failure), ephemeral: true);
                        return;
                    }

                    if (component.Data.CustomId == "eval-unwind-stack")
                    {
                        await component.RespondAsync(embed: EmbedHelper.Build($"Stacktrace: {ex.GetType()}",
                            $"```fix\n{ex.StackTrace}\n```", EmbedHelper.Type.Info), ephemeral: true);
                    }
                };

                await msg.ModifyAsync(x =>
                {
                    x.Embed = EmbedHelper.Build("Evaluation Failure, don't press the button",
                        string.Concat("**", ex.GetType().ToString(), "**: ", ex.Message), EmbedHelper.Type.Failure);
                    x.Components = new ComponentBuilder()
                        .WithButton("Evil button", "eval-unwind-stack", ButtonStyle.Danger)
                        .Build();
                })!;
            }
            finally
            {
                typingState.Dispose();
            }
        }

        private async Task<ScriptState<object>?> EvaluateCodeAsync(string code)
        {
            int codeBlockIndexStart = 0, codeBlockIndexEnd = code.Length;
            if (code.Contains("```"))
            {
                codeBlockIndexStart = code.IndexOf("```", StringComparison.Ordinal) + 3;
                codeBlockIndexEnd = code.LastIndexOf("```", StringComparison.Ordinal);

                if (codeBlockIndexStart == -1 || codeBlockIndexEnd == -1)
                    throw new ArgumentException("You need to wrap the code into a code block.");
            }
            else if (code.Contains("``"))
            {
                codeBlockIndexStart = code.IndexOf("``", StringComparison.Ordinal) + 3;
                codeBlockIndexEnd = code.LastIndexOf("``", StringComparison.Ordinal);

                if (codeBlockIndexStart == -1 || codeBlockIndexEnd == -1)
                    throw new ArgumentException("You need to wrap the code into a code block.");
            }

            var cs = code.Substring(codeBlockIndexStart, codeBlockIndexEnd - codeBlockIndexStart);

            var globals = new EvalVariables(_context);

            var sOpts = ScriptOptions.Default;
            sOpts = sOpts.WithImports("System", "System.Collections.Generic", "System.Linq", "System.Text",
                "System.Threading.Tasks", "Discord", "Discord.Commands");
            sOpts = sOpts.WithReferences(AppDomain.CurrentDomain.GetAssemblies()
                .Where(xa => !xa.IsDynamic && !string.IsNullOrWhiteSpace(xa.Location)));

            var script = CSharpScript.Create(cs, sOpts, typeof(EvalVariables));
            script.Compile();
            var result = await script.RunAsync(globals);

            return result;
        }


    }

    public class EvalVariables
    {
        public IUserMessage Message => this.Context.Message;
        public IMessageChannel Channel => this.Message.Channel;
        public IGuild Guild => this.Context.Guild;
        public IUser User => this.Message.Author;
        public IDiscordClient Client => this.Context.Client;

        public SocketCommandContext Context { get; }

        public EvalVariables(SocketCommandContext ctx)
        {
            this.Context = ctx;
        }
    }

    internal static class EmbedHelper
    {
        public enum Type
        {
            Success,
            Failure,
            Info,
            Warning,
            Waiting
        }

        public static Embed Build(string title, string? description = null, Type? type = null)
        {
            return new EmbedBuilder()
                .WithColor(type switch
                {
                    Type.Success => 0x7FFF00,
                    Type.Failure => 0xFF0000,
                    Type.Info => 0x007FFF,
                    Type.Warning => 0xFFFF00,
                    Type.Waiting => 0x8400FF,
                    _ => 0x000000
                })
                .WithTitle(title)
                .WithDescription(description)
                .Build();
        }
    }
}
