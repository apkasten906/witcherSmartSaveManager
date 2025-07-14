using System;
using System.Configuration;

namespace WitcherGuiApp.Utils
{
    public static class GameSaveExtensions
    {
        public static string GetExtensionForGame(string gameKey)
        {
            string key = $"{gameKey}SaveExtension";
            var ext = ConfigurationManager.AppSettings[key];

            if (!string.IsNullOrWhiteSpace(ext))
                return ext;

            return "*.sav"; // fallback
        }
    }
}
