using System.Diagnostics;

namespace Kimi.GPT2
{
    public class Model
    {
        public static string? Seed { internal get; set; }
        public int Length { internal get; set; }

        public Model(string? seed = null, int length = 5)
        {
            Seed = seed;
            Length = length;
        }

        public string Generate()
        {
            int i = 4;
            Seed = i switch
            {
                (int)PremadeSeed.Capitalismo => PremadeSeed.Capitalismo.DisplayValue()
            };

            return Execute();
        }

        internal static string Execute()
        {
            string generation;
            ProcessStartInfo start = new()
            {
                FileName =
                @"C:\Users\Netty\AppData\Local\Microsoft\WindowsApps\PythonSoftwareFoundation.Python.3.10_qbz5n2kfra8p0\python.exe",
                Arguments = @$"C:\Users\Netty\Desktop\abc.py " + $"\"{Seed}\", 50",
                UseShellExecute = false,
                WorkingDirectory = "",
                RedirectStandardOutput = true
            };

            using Process process = Process.Start(start);
            using StreamReader reader = process.StandardOutput;
            return reader.ReadToEnd();
        }
    }
}