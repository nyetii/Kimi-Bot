using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Kimi.Core.Services
{
    internal class Info
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

            return IsDebug ? attribute.InformationalVersion : $"{attribute.InformationalVersion} REFACTOR";
        }

        public static void GetCommandInfo()
        {
        }
    }
}
