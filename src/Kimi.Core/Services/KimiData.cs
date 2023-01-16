using Kimi.Core.Modules.Monark;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace Kimi.Core.Services
{
    internal class KimiData
    {
        public static dynamic? Monark { get; set; }
        public static List<Datum> TweetData = new();

        public static async void LoadSettings()
        {
            Settings? settings = new();
            var path = $@"{Info.AppDataPath}\settings.kimi";

            if (!File.Exists(path))
                await using (StreamWriter sw = new StreamWriter(path))
                using (JsonWriter writer = new JsonTextWriter(sw))
                {
                    await Logging.LogAsync("Settings file doesn't exist, creating default file...", Severity.Warning);
                    writer.Formatting = Formatting.Indented;
                    JsonSerializer serializer = new JsonSerializer();
                    serializer.Serialize(writer, settings);
                }

            path = await File.ReadAllTextAsync(path);
            JsonConvert.DeserializeObject<Settings>(path);
            await Logging.LogAsync("Settings loaded!");
        }

        public static async Task LoadTweets()
        {
            try
            {
                var path = await File.ReadAllTextAsync(@$"{Info.AppDataPath}\modules\monark\monark.tweets");

                Monark = JsonConvert.DeserializeObject<Root>(path);

                if (Monark != null)
                    foreach (var bulk in Monark.bulk)
                    {
                        foreach (var data in bulk.data)
                        {
                            TweetData.Add(data);
                        }
                    }
            }
            catch (Exception ex)
            {
                await Logging.LogAsync(ex.ToString(), Severity.Fatal);
                Environment.Exit(1);
            }

            await Task.CompletedTask;
        }
    }
}
