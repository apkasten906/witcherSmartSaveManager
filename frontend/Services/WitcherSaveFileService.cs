using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
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

        // Overload to allow for custom save and backup folders, useful for testing or specific configurations
        public WitcherSaveFileService(GameKey gameKey, string saveFolder, string backupFolder)
        {
            this.gameKey = gameKey;
            _saveFolder = saveFolder;
            _backupSaveFolder = backupFolder;
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


        // Overload for WitcherSaveFile object, useful for deletion from ObservableCollection
        public bool DeleteSaveFile(WitcherSaveFile save)
        {
            return DeleteSaveFile(save.FullPath, save.ScreenshotPath);
        }

        // this method needs to be a bit different from backups, because we have to also remove the saves from the OberservableCollection (flow at the ViewModel level)
        // overload for direct file path, useful for deletion without needing to instantiate a WitcherSaveFile object, or testing
        public bool DeleteSaveFile(string fullPath, string screenshotPath = "")
        {
            bool result = false;
            if (string.IsNullOrWhiteSpace(fullPath) || !File.Exists(fullPath))
                throw new FileNotFoundException("Save file does not exist.", fullPath);

            File.Delete(fullPath);

            // Attempt to delete corresponding .bmp screenshot                
            if (File.Exists(screenshotPath))
            {
                File.Delete(screenshotPath);
                result = true;
            }
            return result;
        }

        public bool BackupSaveFile(WitcherSaveFile save, bool overwrite = false)
        {
            if (save == null)
                throw new ArgumentNullException(nameof(save), "Save file cannot be null.");
            return BackupSaveFile(save.FullPath, save.ScreenshotPath, overwrite);
        }

        public bool BackupSaveFile(string fullPath, string screenshotPath = "", bool overwrite = false)
        {
            if (string.IsNullOrWhiteSpace(fullPath) || !File.Exists(fullPath))
                throw new FileNotFoundException("Save file does not exist.", fullPath);

            var destPath = Path.Combine(_backupSaveFolder, Path.GetFileName(fullPath));
            if (!overwrite && File.Exists(destPath))
                return false;  // fail cleanly if not allowed to overwrite

            File.Copy(fullPath, destPath, overwrite);

            if (!string.IsNullOrWhiteSpace(screenshotPath) && File.Exists(screenshotPath))
            {
                var screenshotDestPath = Path.Combine(_backupSaveFolder, Path.GetFileName(screenshotPath));
                if (!overwrite && File.Exists(screenshotDestPath))
                {
                    // optionally skip silently, or return false depending on design
                    return false; // fail cleanly if not allowed to overwrite
                }
                else
                {
                    File.Copy(screenshotPath, screenshotDestPath, overwrite);
                }
            }

            return true;
        }

    }
}

