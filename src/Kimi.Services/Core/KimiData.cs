using Kimi.Logging;
using Newtonsoft.Json;

namespace Kimi.Services.Core
{
    public class KimiData
    {
        public Settings LoadSettings()
        {
            var serializer = new JsonSerializer();
            var settings = new Settings();
            var path = $@"{Info.AppDataPath}\settings.kimi";

            if (!File.Exists(path))
                using (StreamWriter sw = new StreamWriter(path))
                using (JsonWriter writer = new JsonTextWriter(sw))
                {
                    Log.Write("Settings file doesn't exist, creating default file...", Severity.Warning);
                    writer.Formatting = Formatting.Indented;
                    serializer.Serialize(writer, settings);
                }

            using (var sr = new StreamReader(path))
            {
                settings = (Settings?)serializer.Deserialize(sr, typeof(Settings));
                Log.Write("Settings loaded!");
            }

            if(settings == null)
                throw new ArgumentNullException(nameof(settings));

            return settings;
        }

        //public static async Task LoadTweets()
        //{
        //    try
        //    {
        //        using (var sr = new StreamReader(@$"{Info.AppDataPath}\modules\monark\monark.tweets"))
        //        {
        //            JsonSerializer serializer = new ();
        //            Monark = serializer.Deserialize(sr, typeof());
        //        }

        //        Monark = JsonConvert.DeserializeObject<Root>(path);

        //        if (Monark != null)
        //            foreach (var bulk in Monark.bulk)
        //            {
        //                foreach (var data in bulk.data)
        //                {
        //                    TweetData.Add(data);
        //                }
        //            }
        //    }
        //    catch (Exception ex)
        //    {
        //        await Log.Write(ex.ToString(), Severity.Fatal);
        //        Environment.Exit(1);
        //    }

        //    await Task.CompletedTask;
        //}
    }
}
