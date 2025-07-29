using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using WitcherSmartSaveManager.Models;
using WitcherSmartSaveManager.Utils;


namespace WitcherSmartSaveManager.Services
{
    public class WitcherSaveFileService
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        private readonly GameKey gameKey;
        private readonly string _saveFolder;
        public string GetSaveFolder()
        {
            return _saveFolder;
        }
        private string _backupSaveFolder; // Changed from readonly so it can be updated

        /// <summary>
        /// Updates the backup folder path
        /// </summary>
        /// <param name="backupFolder">New backup folder path</param>
        public void UpdateBackupFolder(string backupFolder)
        {
            _backupSaveFolder = backupFolder;
            Logger.Info($"Backup folder updated to: {backupFolder}");
        }

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
            {
                Logger.Warn($"Save folder does not exist: {_saveFolder}");
                return new List<WitcherSaveFile>();
            }

            string extensionPattern = GameSaveExtensions.GetExtensionForGame(gameKey);

            try
            {
                var files = Directory.EnumerateFiles(_saveFolder, extensionPattern, SearchOption.TopDirectoryOnly);
                return files.Select(file =>
                {
                    var info = new FileInfo(file);
                    var saveName = Path.GetFileNameWithoutExtension(info.Name);
                    var screenshotName = saveName + "_640x360.bmp";
                    var screenshotPath = Path.Combine(_saveFolder, screenshotName);
                    var backupPath = Path.Combine(_backupSaveFolder, info.Name);
                    var backupScreenshotPath = Path.Combine(_backupSaveFolder, screenshotName);
                    Logger.Debug($"Found save: {info.Name}, Modified: {info.LastWriteTimeUtc}");
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
            catch (Exception ex)
            {
                Logger.Error(ex, "Error enumerating save files.");
                return new List<WitcherSaveFile>();
            }
        }

        /// <summary>
        /// Gets the current backup folder path (for debugging)
        /// </summary>
        /// <returns>Current backup folder path</returns>
        public string GetBackupFolder()
        {
            return _backupSaveFolder;
        }

        /// <summary>
        /// Gets the total number of backup files in the backup folder
        /// </summary>
        /// <returns>Count of save files in the backup folder using the game-specific extension</returns>
        public int GetBackupFileCount()
        {
            try
            {
                if (!Directory.Exists(_backupSaveFolder))
                {
                    Logger.Debug($"Backup folder does not exist: {_backupSaveFolder}");
                    return 0;
                }

                // Use the same extension pattern as GetSaveFiles
                string extensionPattern = GameSaveExtensions.GetExtensionForGame(gameKey);
                Logger.Debug($"Using extension pattern: {extensionPattern} for game: {gameKey}");

                var backupFiles = Directory.EnumerateFiles(_backupSaveFolder, extensionPattern, SearchOption.TopDirectoryOnly);
                var count = backupFiles.Count();
                Logger.Debug($"Found {count} backup files in {_backupSaveFolder}");
                return count;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"Error counting backup files in {_backupSaveFolder}");
                return 0;
            }
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
            bool saveDeleted = false;
            if (string.IsNullOrWhiteSpace(fullPath) || !File.Exists(fullPath))
                return false;

            try
            {
                File.Delete(fullPath);
                saveDeleted = true;
                Logger.Info($"Successfully deleted save file: {fullPath}");
            }
            catch (IOException ex) when (ex.Message.Contains("being used by another process"))
            {
                Logger.Warn($"Save file is in use, could not delete: {fullPath}. Error: {ex.Message}");
                return false; // Save file deletion failed, don't try screenshot
            }
            catch (UnauthorizedAccessException ex)
            {
                Logger.Warn($"Access denied when deleting save file: {fullPath}. Error: {ex.Message}");
                return false; // Save file deletion failed, don't try screenshot
            }

            // Attempt to delete corresponding .bmp screenshot - but only if save deletion succeeded
            if (!string.IsNullOrWhiteSpace(screenshotPath) && File.Exists(screenshotPath))
            {
                try
                {
                    File.Delete(screenshotPath);
                    Logger.Info($"Successfully deleted screenshot: {screenshotPath}");
                }
                catch (IOException ex) when (ex.Message.Contains("being used by another process"))
                {
                    Logger.Warn($"Screenshot is in use, will become orphaned: {screenshotPath}. Error: {ex.Message}");
                    // Don't fail the operation - save was deleted successfully, screenshot will be orphaned
                }
                catch (UnauthorizedAccessException ex)
                {
                    Logger.Warn($"Access denied when deleting screenshot: {screenshotPath}. Error: {ex.Message}");
                    // Don't fail the operation - save was deleted successfully, screenshot will be orphaned
                }
            }

            return saveDeleted;
        }

        public bool BackupSaveFile(WitcherSaveFile save, bool overwrite = false)
        {
            Logger.Info($"Backing up save file: {save?.FullPath}");
            if (save == null)
                throw new ArgumentNullException(nameof(save), "Save file cannot be null.");
            return BackupSaveFile(save.FullPath, save.ScreenshotPath, overwrite);
        }

        public bool BackupSaveFile(string fullPath, string screenshotPath = "", bool overwrite = false)
        {
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

        /// <summary>
        /// Finds orphaned screenshot files that don't have corresponding save files
        /// </summary>
        /// <returns>List of orphaned screenshot file paths</returns>
        public List<string> GetOrphanedScreenshots()
        {
            var orphanedScreenshots = new List<string>();

            if (!Directory.Exists(_saveFolder))
            {
                Logger.Warn($"Save folder does not exist: {_saveFolder}");
                return orphanedScreenshots;
            }

            try
            {
                // Get all BMP files in the save directory
                var bmpFiles = Directory.GetFiles(_saveFolder, "*.bmp", SearchOption.TopDirectoryOnly);

                Logger.Info($"Found {bmpFiles.Length} BMP files in save directory");

                foreach (var bmpFile in bmpFiles)
                {
                    var bmpFileName = Path.GetFileNameWithoutExtension(bmpFile);

                    // Check if this looks like a Witcher 2 screenshot (ends with _640x360)
                    if (bmpFileName.EndsWith("_640x360"))
                    {
                        // Extract the save name by removing the screenshot suffix
                        var saveBaseName = bmpFileName.Substring(0, bmpFileName.Length - "_640x360".Length);

                        // Look for corresponding save file
                        string extensionPattern = GameSaveExtensions.GetExtensionForGame(gameKey);
                        var saveExtension = extensionPattern.Replace("*", "");
                        var expectedSaveFile = Path.Combine(_saveFolder, saveBaseName + saveExtension);

                        if (!File.Exists(expectedSaveFile))
                        {
                            orphanedScreenshots.Add(bmpFile);
                            Logger.Debug($"Found orphaned screenshot: {bmpFile} (no corresponding save: {expectedSaveFile})");
                        }
                    }
                }

                Logger.Info($"Found {orphanedScreenshots.Count} orphaned screenshots");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error while detecting orphaned screenshots");
            }

            return orphanedScreenshots;
        }

        /// <summary>
        /// Deletes orphaned screenshot files
        /// </summary>
        /// <param name="orphanedScreenshots">List of orphaned screenshot paths to delete</param>
        /// <returns>Number of successfully deleted files</returns>
        public int CleanupOrphanedScreenshots(List<string> orphanedScreenshots)
        {
            int deletedCount = 0;

            if (orphanedScreenshots == null || orphanedScreenshots.Count == 0)
            {
                Logger.Info("No orphaned screenshots to clean up");
                return deletedCount;
            }

            Logger.Info($"Attempting to clean up {orphanedScreenshots.Count} orphaned screenshots");

            foreach (var orphanedFile in orphanedScreenshots)
            {
                try
                {
                    if (File.Exists(orphanedFile))
                    {
                        File.Delete(orphanedFile);
                        deletedCount++;
                        Logger.Info($"Successfully deleted orphaned screenshot: {orphanedFile}");
                    }
                }
                catch (IOException ex) when (ex.Message.Contains("being used by another process"))
                {
                    Logger.Warn($"Orphaned screenshot is in use, skipping: {orphanedFile}. Error: {ex.Message}");
                }
                catch (UnauthorizedAccessException ex)
                {
                    Logger.Warn($"Access denied when deleting orphaned screenshot: {orphanedFile}. Error: {ex.Message}");
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, $"Unexpected error deleting orphaned screenshot: {orphanedFile}");
                }
            }

            Logger.Info($"Successfully cleaned up {deletedCount} of {orphanedScreenshots.Count} orphaned screenshots");
            return deletedCount;
        }

        /// <summary>
        /// Convenience method to detect and clean up all orphaned screenshots
        /// </summary>
        /// <returns>Number of successfully deleted files</returns>
        public int CleanupAllOrphanedScreenshots()
        {
            var orphanedScreenshots = GetOrphanedScreenshots();
            return CleanupOrphanedScreenshots(orphanedScreenshots);
        }

        /// <summary>
        /// Attempts to identify what might be locking a file
        /// </summary>
        /// <param name="filePath">Path to the file to check</param>
        /// <returns>Helpful message about potential locking processes</returns>
        private string GetFileLockInfo(string filePath)
        {
            try
            {
                // Try to open file exclusively to see if it's locked
                using (var stream = File.Open(filePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
                {
                    return "File is not locked";
                }
            }
            catch (IOException ex) when (ex.Message.Contains("being used by another process"))
            {
                // Try to identify common culprits
                var fileName = Path.GetFileName(filePath);
                var processes = System.Diagnostics.Process.GetProcesses();

                var suspiciousProcesses = processes
                    .Where(p =>
                    {
                        try
                        {
                            var name = p.ProcessName.ToLower();
                            return name.Contains("witcher") ||
                                   name.Contains("cdpr") ||
                                   name.Contains("gog") ||
                                   name.Contains("steam") ||
                                   name.Contains("explorer"); // Windows Explorer can lock files when viewing them
                        }
                        catch { return false; }
                    })
                    .Select(p =>
                    {
                        try { return p.ProcessName; }
                        catch { return "Unknown"; }
                    })
                    .Distinct()
                    .ToList();

                if (suspiciousProcesses.Any())
                {
                    return $"File may be locked by: {string.Join(", ", suspiciousProcesses)}. Try closing these applications.";
                }

                return "File is locked by another process. Try closing Witcher 2, GOG Galaxy, Steam, or any file explorers viewing the save folder.";
            }
            catch (UnauthorizedAccessException)
            {
                return "Access denied. Run as administrator or check file permissions.";
            }
            catch (Exception ex)
            {
                return $"Unable to check file lock status: {ex.Message}";
            }
        }

        /// <summary>
        /// Enhanced cleanup that provides better feedback about locked files
        /// </summary>
        /// <param name="orphanedScreenshots">List of orphaned screenshot paths to delete</param>
        /// <returns>Tuple of (successCount, lockedFiles with reasons)</returns>
        public (int deletedCount, List<(string file, string reason)> lockedFiles) CleanupOrphanedScreenshotsWithDetails(List<string> orphanedScreenshots)
        {
            int deletedCount = 0;
            var lockedFiles = new List<(string file, string reason)>();

            if (orphanedScreenshots == null || orphanedScreenshots.Count == 0)
            {
                Logger.Info("No orphaned screenshots to clean up");
                return (deletedCount, lockedFiles);
            }

            Logger.Info($"Attempting to clean up {orphanedScreenshots.Count} orphaned screenshots");

            foreach (var orphanedFile in orphanedScreenshots)
            {
                try
                {
                    if (File.Exists(orphanedFile))
                    {
                        File.Delete(orphanedFile);
                        deletedCount++;
                        Logger.Info($"Successfully deleted orphaned screenshot: {orphanedFile}");
                    }
                }
                catch (IOException ex) when (ex.Message.Contains("being used by another process"))
                {
                    var lockInfo = GetFileLockInfo(orphanedFile);
                    lockedFiles.Add((orphanedFile, lockInfo));
                    Logger.Warn($"Orphaned screenshot is locked: {orphanedFile}. {lockInfo}");
                }
                catch (UnauthorizedAccessException ex)
                {
                    var reason = "Access denied - check permissions or run as administrator";
                    lockedFiles.Add((orphanedFile, reason));
                    Logger.Warn($"Access denied when deleting orphaned screenshot: {orphanedFile}. Error: {ex.Message}");
                }
                catch (Exception ex)
                {
                    var reason = $"Unexpected error: {ex.Message}";
                    lockedFiles.Add((orphanedFile, reason));
                    Logger.Error(ex, $"Unexpected error deleting orphaned screenshot: {orphanedFile}");
                }
            }

            Logger.Info($"Successfully cleaned up {deletedCount} of {orphanedScreenshots.Count} orphaned screenshots");
            return (deletedCount, lockedFiles);
        }
    }
}

