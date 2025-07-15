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
        private readonly GameKey gameKey;
        private readonly string _saveFolder;
        private readonly string _backupSaveFolder;

        public WitcherSaveFileService(GameKey gameKey)
        {
            this.gameKey = gameKey;
            _saveFolder = SavePathResolver.GetSavePath(gameKey);
            _backupSaveFolder = SavePathResolver.GetDefaultBackupPath(gameKey);
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
                var screenshotName = saveName + "_640x360.bmp"; // Assuming the screenshot follows this naming convention
                var screenshotPath = Path.Combine(_saveFolder, screenshotName);
                var backupPath = Path.Combine(_backupSaveFolder, info.Name);
                var backupScreenshotPath = Path.Combine(_backupSaveFolder, screenshotName);


                return new WitcherSaveFile
                {
                    Game = gameKey,
                    FileName = info.Name,
                    ModifiedTime = new DateTimeOffset(info.LastWriteTimeUtc).ToUnixTimeSeconds(),
                    ModifiedTimeIso = info.LastWriteTimeUtc.ToString("o"),
                    Size = (int)info.Length,
                    FullPath = info.FullName,
                    ScreenshotPath = File.Exists(screenshotPath) ? screenshotPath : string.Empty,
                    BackupExists = File.Exists(backupPath),
                    Metadata = MetadataExtractor.GetMetadata(file)
                };
            }).ToList();
        }

        // this method needs to be a bit different from backups, because we have to also remove the saves from the OberservableCollection (flow at the ViewModel level)
        public bool DeleteSaveFile(WitcherSaveFile save)
        {            
            if (string.IsNullOrWhiteSpace(save.FullPath) || !File.Exists(save.FullPath))
                throw new FileNotFoundException("Save file does not exist.", save.FullPath);

            File.Delete(save.FullPath);

            // Attempt to delete corresponding .bmp screenshot                
            if (File.Exists(save.ScreenshotPath))
            {
                File.Delete(save.ScreenshotPath);
            }
            return true;
        }

        public string BackupSaveFile(WitcherSaveFile save, bool overwrite = false)
        {     
            try
            {
                if (string.IsNullOrWhiteSpace(save.FullPath) || !File.Exists(save.FullPath))
                    throw new FileNotFoundException("Save file does not exist.", save.FullPath);

                File.Copy(save.FullPath, Path.Combine(_backupSaveFolder, save.FileName), overwrite);

                if (!string.IsNullOrWhiteSpace(save.ScreenshotPath) && File.Exists(save.ScreenshotPath))
                {
                    string screenshotBackupPath = Path.Combine(_backupSaveFolder, Path.GetFileName(save.ScreenshotPath));                    
                    File.Copy(save.ScreenshotPath, screenshotBackupPath, overwrite);
                }
                return "success";
                    
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error backing up file: {ex.Message}");
                return ex.Message;
            }
        }
    }    
}

