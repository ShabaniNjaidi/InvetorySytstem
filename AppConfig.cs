using System;
using System.IO;
using System.Text.Json;

namespace InventorySystem.Helpers  // use your namespace
{
    public class AppConfig
    {
        public string ImageFolderPath { get; set; }

        // ✅ New: Remembered Username
        public string RememberedUsername { get; set; }
    }

    public static class ConfigManager
    {
        private static readonly string configFilePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "InventorySystem",  // change this to your app name/folder
            "config.json");

        public static void SaveConfig(AppConfig config)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(configFilePath));
            string json = JsonSerializer.Serialize(config);
            File.WriteAllText(configFilePath, json);
        }

        public static AppConfig LoadConfig()
        {
            if (!File.Exists(configFilePath)) return null;

            string json = File.ReadAllText(configFilePath);
            return JsonSerializer.Deserialize<AppConfig>(json);
        }
    }
}
