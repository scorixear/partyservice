using Newtonsoft.Json;
using System.IO;
using System.Reflection;

namespace PartyService
{
    public class Config
    {
        public static string BaseLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        public static Config instance = Instantation();

        private static Config Instantation()
        {
            Config returnInstance;
            if (File.Exists(Path.Combine(BaseLocation, "Config.json")))
            {
                returnInstance = JsonConvert.DeserializeObject<Config>(File.ReadAllText(Path.Combine(BaseLocation, "Config.json")));
            }
            else
            {
                returnInstance = new Config();
            }
            returnInstance.Save();
            return returnInstance;
        }

        private void Save()
        {
            File.WriteAllText(Path.Combine(BaseLocation, "Config.json"), JsonConvert.SerializeObject(this, Formatting.Indented));
        }

        private Config() { }
        public string EventsUrl { get; set; } = "https://kellerus.de/lootlogger/events.json";
    }
}
