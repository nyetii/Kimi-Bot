using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using Discord;
using Serilog;
using Serilog.Events;

namespace Kimi.Logging
{
    public class Log
    {
        public static async Task Write(LogMessage message)
        {
            var severity = message.Severity switch
            {
                LogSeverity.Critical => LogEventLevel.Fatal,
                LogSeverity.Error => LogEventLevel.Error,
                LogSeverity.Warning => LogEventLevel.Warning,
                LogSeverity.Info => LogEventLevel.Information,
                LogSeverity.Verbose => LogEventLevel.Verbose,
                LogSeverity.Debug => LogEventLevel.Debug,
                _ => LogEventLevel.Information
            };

            Serilog.Log.Write(severity, message.Exception, "[{Source}] {Message}", message.Source, message.Message);
            await Task.CompletedTask;
        }

        /// <summary>
        /// Writes log information in behalf of the bot.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="severity"></param>
        /// <returns></returns>
        public static async Task Write(string message, Severity severity = Severity.Info, [CallerFilePath] string? module = null)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            module ??= "Kimi";
            string[] names = module.Split(@"\");
            foreach (var name in names)
            {
                if (!name.Contains("Kimi.")) continue;
                names = name.Split('.');
                module = names[1];
            }

            stopwatch.Stop();
            Console.WriteLine($"Elapsed - {stopwatch.Elapsed.TotalNanoseconds:00} ns");
            Serilog.Log.Write((LogEventLevel)severity, "[{Source}] {Message}", $"{module}", message);
            await Task.CompletedTask;
        }
    }

    public enum Severity
    {
        Fatal = LogEventLevel.Fatal,
        Error = LogEventLevel.Error,
        Warning = LogEventLevel.Warning,
        Info = LogEventLevel.Information,
        Verbose = LogEventLevel.Verbose,
        Debug = LogEventLevel.Debug
    }
}