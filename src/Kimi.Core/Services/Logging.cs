using Discord;
using Serilog;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kimi.Core.Services
{
    internal class Logging
    {
        internal static async Task LogAsync(LogMessage message)
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

            Log.Write(severity, message.Exception, "[{Source}] {Message}", message.Source, message.Message);
            await Task.CompletedTask;
        }

        internal static async Task LogAsync(string message, Severity severity = Severity.Info)
        {
            Log.Write((LogEventLevel)severity, "[{Source}] {Message}", "Kimi", message);
            await Task.CompletedTask;
        }
    }

    enum Severity
    {
        Fatal = LogEventLevel.Fatal,
        Error = LogEventLevel.Error,
        Warning = LogEventLevel.Warning,
        Info = LogEventLevel.Information,
        Verbose = LogEventLevel.Verbose,
        Debug = LogEventLevel.Debug
    }
}
