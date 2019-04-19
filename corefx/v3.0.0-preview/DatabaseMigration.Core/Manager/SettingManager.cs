using Newtonsoft.Json;
using System.IO;

namespace DatabaseMigration.Core
{
    public class SettingManager
    {
        public static Setting Setting { get; set; } = new Setting();

        static SettingManager()
        {
            LoadConfig();
        }
            

        public static string ConfigFilePath
        {
            get
            {
                string configRootFolder = Path.Combine(PathHelper.GetAssemblyFolder(), "Configs");               
                return Path.Combine(configRootFolder, "Setting.json");
            }
        }

        public static void LoadConfig()
        {
            Setting = (Setting)JsonConvert.DeserializeObject(File.ReadAllText(ConfigFilePath), typeof(Setting));
        }

        public static void SaveConfig(Setting setting)
        {
            Setting = setting;
            string content = JsonConvert.SerializeObject(setting, Formatting.Indented);
            File.WriteAllText(ConfigFilePath, content);
        }
    }
}
