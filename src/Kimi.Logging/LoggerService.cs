using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Serilog;
using Serilog.Formatting;

namespace Kimi.Logging
{
    public class LoggerService
    {
        public async Task LoggerConfiguration(string? path)
        {
            path ??= Environment.CurrentDirectory;
            Serilog.Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .WriteTo.File(@$"{path}\logs\kimi.log", rollingInterval: RollingInterval.Day,
                    outputTemplate: "[{Timestamp:yyyy-MM-dd - HH:mm:ss.fff}|{Level:u3}] {Message:lj}{NewLine}{Exception}")
                .CreateLogger();

            await Task.CompletedTask;
        }
    }
}
