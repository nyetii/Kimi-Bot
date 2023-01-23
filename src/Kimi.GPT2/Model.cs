using System.Diagnostics;
using System.Net;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Kimi.GPT2
{
    public class Model
    {
        public string? Seed { internal get; set; }
        public int Length { internal get; set; }
        private static readonly string Path = IModelData.Path;

        public Model(string? seed = null, int length = 5)
        {
            Seed = seed;
            Length = length;
        }

        public static async Task<bool> IsReady()
        {
            try
            {
                IModelData modeldata = new ModelData();
                if (!Directory.Exists(Path))
                    Directory.CreateDirectory(Path);

                if (!File.Exists($@"{Path}\generate.py"))
                    await modeldata.DownloadPythonFile();

                if (!Directory.Exists($@"{Path}\model"))
                    await modeldata.DownloadModel();
                
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        

        public async Task<string> Generate()
        {
            Random rng = new();
            var i = (PremadeSeed)rng.Next(0, PremadeSeed.Alexandre.Length());
            Seed = i switch
            {
                PremadeSeed.Capitalismo => PremadeSeed.Capitalismo.DisplayValue(),
                PremadeSeed.Alexandre => PremadeSeed.Alexandre.DisplayValue(),
                PremadeSeed.Bolsonaro => PremadeSeed.Bolsonaro.DisplayValue(),
                PremadeSeed.Ditadura => PremadeSeed.Ditadura.DisplayValue(),
                PremadeSeed.Dolar => PremadeSeed.Dolar.DisplayValue(),
                PremadeSeed.ElonMusk => PremadeSeed.ElonMusk.DisplayValue(),
                PremadeSeed.Flow => PremadeSeed.Flow.DisplayValue(),
                PremadeSeed.Liberdade => PremadeSeed.Liberdade.DisplayValue(),
                PremadeSeed.Lula => PremadeSeed.Lula.DisplayValue(),
                PremadeSeed.Maconha => PremadeSeed.Maconha.DisplayValue(),
                PremadeSeed.STF => PremadeSeed.Maconha.DisplayValue(),
                PremadeSeed.Void => null,
                _ => null
            };

            return await Execute();
        }

        internal async Task<string> Execute()
        {
            var path = $@"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\Kimi\GPT2";
            ProcessStartInfo start = new()
            {
                FileName =
                @"C:\Users\Netty\AppData\Local\Microsoft\WindowsApps\PythonSoftwareFoundation.Python.3.10_qbz5n2kfra8p0\python.exe",
                Arguments = @$"{path}\generate.py " + $"\"{Seed}\", 50",
                UseShellExecute = false,
                WorkingDirectory = "",
                RedirectStandardOutput = true
            };

            using Process process = Process.Start(start);
            using StreamReader reader = process.StandardOutput;
            var generation = await reader.ReadToEndAsync();


            if (!generation.Contains("Traceback"))
                return await Task.FromResult(await Generation.Parse(generation, Seed));

            await Execute();
            throw new Exception();
        }
    }
}