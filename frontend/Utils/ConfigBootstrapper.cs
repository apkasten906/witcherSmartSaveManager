using System;
using System.Configuration;
using System.IO;
using WitcherSmartSaveManager.Models;

namespace WitcherSmartSaveManager.Utils
{
    public static class ConfigBootstrapper
    {
        private const string ConfigFileName = "userpaths.json";

        public static bool EnsureConfigFileExists(out string resultMessage)
        {
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ConfigFileName);

            if (File.Exists(path))
            {
                resultMessage = $"Config file found at {path}.";
                return true;
            }

            try
            {
                string defaultW1 = ConfigurationManager.AppSettings["Witcher1DefaultSaveFolder"] ?? "";
                string defaultW2 = ConfigurationManager.AppSettings["Witcher2DefaultSaveFolder"] ?? "";
                string defaultW3 = ConfigurationManager.AppSettings["Witcher3DefaultSaveFolder"] ?? "";

                string jsonWithComments = @$"// This file allows you to override default save game paths.
                        // Modify only the values. Do not remove the top-level key: SavePaths.
                        // SavePaths will override the App.config values if present.

                        {{
                          ""SavePaths"": {{
                            ""{GameKey.Witcher1}"": ""{defaultW1}"",
                            ""{GameKey.Witcher2}"": ""{defaultW2}"",
                            ""{GameKey.Witcher3}"": ""{defaultW3}""
                          }}
                        }}
                        ";
                File.WriteAllText(path, jsonWithComments);
                resultMessage = $"Created default config file at: {path}";
                return true;
            }
            catch (Exception ex)
            {
                resultMessage = $"Failed to create config file '{path}': {ex.Message}";
                return false;
            }
        }

    }
}
