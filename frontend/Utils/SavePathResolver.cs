#nullable enable
using System;
using Microsoft.Extensions.Configuration;
using WitcherGuiApp.Models;

namespace WitcherGuiApp.Utils
{
    public static class SavePathResolver
    {
        private static readonly IConfiguration? _userConfig;

        static SavePathResolver()
        {
            try
            {
                _userConfig = new ConfigurationBuilder()
                    .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                    .AddJsonFile("userpaths.json", optional: true)
                    .Build();
            }
            catch
            {
                _userConfig = null;
            }
        }

        public static string GetSavePath(GameKey gameKey)
        {
            var userPath = _userConfig?[$"SavePaths:{gameKey}"];
            if (!string.IsNullOrWhiteSpace(userPath))
                return Environment.ExpandEnvironmentVariables(userPath);

            var defaultPath = System.Configuration.ConfigurationManager.AppSettings[$"{gameKey}DefaultSaveFolder"];
            if (!string.IsNullOrWhiteSpace(defaultPath))
                return Environment.ExpandEnvironmentVariables(defaultPath);

            throw new Exception($"No save path configured for game key '{gameKey}'");
        }

        public static string GetDefaultBackupPath(GameKey gameKey)
        {
            var userBackupPath = _userConfig?[$"BackupPaths:{gameKey}"];
            if (!string.IsNullOrWhiteSpace(userBackupPath))
                return Environment.ExpandEnvironmentVariables(userBackupPath);
            // Reasonable fallback
            return $"{GetSavePath(gameKey)}\\_backup";
        }

        public static string GetSaveExtension(GameKey gameKey)
        {
            var extension = _userConfig?[$"SaveExtensions:{gameKey}"];
            if (!string.IsNullOrWhiteSpace(extension))
                return extension;

            // Reasonable fallback
            return "*.sav";
        }
    }
}
