using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using WitcherGuiApp.Models;
using WitcherGuiApp.Utils;

namespace WitcherGuiApp.Services
{
    public class WitcherSaveFileService
    {
        private readonly string _gameKey;
        private readonly string _saveFolder;

        public WitcherSaveFileService(string gameKey)
        {
            _gameKey = gameKey;
            _saveFolder = SavePathResolver.GetSavePath(gameKey);
        }


        public List<SaveFile> GetSaveFiles()
    {
        if (!Directory.Exists(_saveFolder))
            return new List<SaveFile>();

        string extensionPattern = GameSaveExtensions.GetExtensionForGame(_gameKey);

        var files = Directory.EnumerateFiles(_saveFolder, extensionPattern, SearchOption.TopDirectoryOnly);

        return files.Select(file =>
        {
            var info = new FileInfo(file);
            var saveName = Path.GetFileNameWithoutExtension(info.Name);
            var screenshotPath = Path.Combine(_saveFolder, saveName + ".bmp");


            return new SaveFile
            {
                Game = _gameKey,
                FileName = info.Name,
                ModifiedTime = new DateTimeOffset(info.LastWriteTimeUtc).ToUnixTimeSeconds(),
                ModifiedTimeIso = info.LastWriteTimeUtc.ToString("o"),
                Size = (int)info.Length,
                FullPath = info.FullName,
                ScreenshotPath = File.Exists(screenshotPath) ? screenshotPath : string.Empty,
                Metadata = MetadataExtractor.GetMetadata(file)
            };
        }).ToList();
    }


    public bool DeleteSaveFile(string fullPath)
        {
            if (string.IsNullOrWhiteSpace(fullPath) || !File.Exists(fullPath))
                return false;

            try
            {
                File.Delete(fullPath);

                // Attempt to delete corresponding .bmp screenshot
                var bmpPath = Path.Combine(Path.GetDirectoryName(fullPath) ?? "", Path.GetFileNameWithoutExtension(fullPath) + ".bmp");
                if (File.Exists(bmpPath))
                {
                    File.Delete(bmpPath);
                }

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
