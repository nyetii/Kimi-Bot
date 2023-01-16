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
        public static Root Monark { get; set; }
        public static List<Datum> TweetData = new List<Datum>();
        
        public static async void LoadSettings()
        {
            Settings? settings = new();
            var path = $@"{Info.AppDataPath}\settings.kimi";

            if (!File.Exists(path))
                using (StreamWriter sw = new StreamWriter(path))
                using (JsonWriter writer = new JsonTextWriter(sw))
                {
                    await Logging.LogAsync("Settings file doesn't exist, creating default file...", Severity.Warning);
                    writer.Formatting = Formatting.Indented;
                    JsonSerializer serializer = new JsonSerializer();
                    serializer.Serialize(writer, settings);
                }

            path = File.ReadAllText(path);
            JsonConvert.DeserializeObject<Settings>(path);
            await Logging.LogAsync("Settings loaded!");
        }

        public static async Task LoadTweets()
        {
            var path = File.ReadAllText(@$"{Info.AppDataPath}\modules\monark\monark.json");

            //using(StreamReader sr = new StreamReader(path))
            //using(JsonReader reader = new JsonTextReader(sr))
            //{
            //    reader.SupportMultipleContent = true;

            //    var serializer = new JsonSerializer();

            //    while(reader.Read())
            //    serializer.Deserialize<Root>(reader);
            //}

            Monark = JsonConvert.DeserializeObject<Root>(path);

            foreach(var bulk in Monark.bulk)
            {
                foreach(var data in bulk.data)
                {
                    TweetData.Add(data);
                }
            }

            await Task.CompletedTask;
        }
    }
}
