using Kimi.Logging;
using Newtonsoft.Json;

namespace Kimi.Services.Core
{
    public class KimiData
    {
        public static dynamic? Monark { get; set; }
        public static List<dynamic> TweetData = new();

        public static async Task<Settings?> LoadSettings()
        {
            Settings? settings = new();
            var path = $@"{Info.AppDataPath}\settings.kimi";

            if (!File.Exists(path))
                await using (StreamWriter sw = new StreamWriter(path))
                using (JsonWriter writer = new JsonTextWriter(sw))
                {
                    await Log.Write("Settings file doesn't exist, creating default file...", Severity.Warning);
                    writer.Formatting = Formatting.Indented;
                    JsonSerializer serializer = new JsonSerializer();
                    serializer.Serialize(writer, settings);
                }

            path = await File.ReadAllTextAsync(path);
            settings = JsonConvert.DeserializeObject<Settings>(path);
            await Log.Write("Settings loaded!");

            return await Task.FromResult(settings);
        }

        public static async Task LoadTweets()
        {
            //try
            //{
            //    var path = await File.ReadAllTextAsync(@$"{Info.AppDataPath}\modules\monark\monark.tweets");

            //    Monark = JsonConvert.DeserializeObject<Root>(path);

            //    if (Monark != null)
            //        foreach (var bulk in Monark.bulk)
            //        {
            //            foreach (var data in bulk.data)
            //            {
            //                TweetData.Add(data);
            //            }
            //        }
            //}
            //catch (Exception ex)
            //{
            //    await Logging.LogAsync(ex.ToString(), Severity.Fatal);
            //    Environment.Exit(1);
            //}

            await Task.CompletedTask;
        }
    }
}
