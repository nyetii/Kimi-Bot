using System.Diagnostics;
using System.Reflection;

namespace Kimi.Services.Core
{
    public class Info
    {
        public static bool IsDebug { get; set; } = false;
        public static string Version { get; private set; } = GetVersionInfo();
        public static string AppDataPath { get; private set; } = $@"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\Kimi";
        public static Dictionary<string, dynamic> CommandInfo = new();


        private static string GetVersionInfo()
        {
            var attribute = (AssemblyInformationalVersionAttribute)Assembly.GetExecutingAssembly()
                .GetCustomAttributes(typeof(AssemblyInformationalVersionAttribute), false)
                .FirstOrDefault();

            return attribute.InformationalVersion;
        }

        public static void GetCommandInfo()
        {
        }
    }
}
