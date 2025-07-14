using System;
using System.CodeDom;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using WitcherGuiApp.Models;
using WitcherGuiApp.Utils;

namespace WitcherGuiApp.Services
{
    public class WitcherSaveFileService
    {
        private readonly string gameKey;
        private readonly string _saveFolder;

    public WitcherSaveFileService(string gameKey)
    {
        this.gameKey = gameKey;
        _saveFolder = SavePathResolver.GetSavePath(gameKey);
    }


    public List<WitcherSaveFile> GetSaveFiles()
    {
        if (!Directory.Exists(_saveFolder))
            return new List<WitcherSaveFile>();

        string extensionPattern = GameSaveExtensions.GetExtensionForGame(gameKey);

        var files = Directory.EnumerateFiles(_saveFolder, extensionPattern, SearchOption.TopDirectoryOnly);

        return files.Select(file =>
        {
            var info = new FileInfo(file);
            var saveName = Path.GetFileNameWithoutExtension(info.Name);
            var screenshotPath = Path.Combine(_saveFolder, saveName + ".bmp");


            return new WitcherSaveFile
            {
                Game = gameKey,
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

        
        public bool BackupSaveFile(string fullPath, string gamekey, string backupFolderPath = "", bool overwriteAll = false)
        {
            if (string.IsNullOrWhiteSpace(fullPath) || !File.Exists(fullPath))
                return false;

            try
            {                
                File.Copy(fullPath, backupFolderPath);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error backing up file: {ex.Message}");
                return false;
            }
            return true;
        }
    }    
}

