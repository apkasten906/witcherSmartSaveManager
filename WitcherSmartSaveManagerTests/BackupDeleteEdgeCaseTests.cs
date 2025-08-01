using NUnit.Framework;
using WitcherCore.Services;
using WitcherCore.Models;
using System;
using System.IO;

namespace WitcherSmartSaveManager.Tests
{
    [TestFixture]
    public class BackupDeleteEdgeCaseTests
    {
        [Test]
        public void BackupSaveFile_OverwriteBehavior()
        {
            var tempDir = Path.Combine(Path.GetTempPath(), "WitcherTestBackup");
            var backupDir = Path.Combine(Path.GetTempPath(), "WitcherTestBackup_backup");
            if (Directory.Exists(tempDir)) Directory.Delete(tempDir, true);
            if (Directory.Exists(backupDir)) Directory.Delete(backupDir, true);
            Directory.CreateDirectory(tempDir);
            Directory.CreateDirectory(backupDir);
            var testFile = Path.Combine(tempDir, "test.sav");
            File.WriteAllText(testFile, "original");
            var service = new WitcherSaveFileService(GameKey.Witcher2, tempDir, backupDir);
            var save = new WitcherSaveFile
            {
                FileName = "test.sav",
                FullPath = testFile,
                ScreenshotPath = "",
                ModifiedTimeIso = DateTime.Now.ToString("O")
            };
            // First backup
            Assert.That(service.BackupSaveFile(save, false), Is.True);
            // Overwrite backup
            File.WriteAllText(testFile, "newdata");
            Assert.That(service.BackupSaveFile(save, true), Is.True);
            Directory.Delete(tempDir, true);
            Directory.Delete(backupDir, true);
        }

        [Test]
        public void BackupSaveFile_NoOverwrite_ReturnsFalse()
        {
            var tempDir = Path.Combine(Path.GetTempPath(), "WitcherTestBackupNoOverwrite");
            var backupDir = Path.Combine(Path.GetTempPath(), "WitcherTestBackupNoOverwrite_backup");
            if (Directory.Exists(tempDir)) Directory.Delete(tempDir, true);
            if (Directory.Exists(backupDir)) Directory.Delete(backupDir, true);
            Directory.CreateDirectory(tempDir);
            Directory.CreateDirectory(backupDir);
            var testFile = Path.Combine(tempDir, "test.sav");
            File.WriteAllText(testFile, "original");
            var service = new WitcherSaveFileService(GameKey.Witcher2, tempDir, backupDir);
            var save = new WitcherSaveFile
            {
                FileName = "test.sav",
                FullPath = testFile,
                ScreenshotPath = "",
                ModifiedTimeIso = DateTime.Now.ToString("O")
            };
            // First backup
            Assert.That(service.BackupSaveFile(save, false), Is.True);
            // Try backup again without overwrite
            Assert.That(service.BackupSaveFile(save, false), Is.False);
            Directory.Delete(tempDir, true);
            Directory.Delete(backupDir, true);
        }

        [Test]
        public void DeleteSaveFile_MissingFile_HandlesGracefully()
        {
            var tempDir = Path.Combine(Path.GetTempPath(), "WitcherTestDelete");
            var backupDir = Path.Combine(Path.GetTempPath(), "WitcherTestDelete_backup");
            if (Directory.Exists(tempDir)) Directory.Delete(tempDir, true);
            if (Directory.Exists(backupDir)) Directory.Delete(backupDir, true);
            Directory.CreateDirectory(tempDir);
            Directory.CreateDirectory(backupDir);
            var testFile = Path.Combine(tempDir, "missing.sav");
            var service = new WitcherSaveFileService(GameKey.Witcher2, tempDir, backupDir);
            var save = new WitcherSaveFile
            {
                FileName = "missing.sav",
                FullPath = testFile,
                ScreenshotPath = "",
                ModifiedTimeIso = DateTime.Now.ToString("O")
            };
            Assert.DoesNotThrow(() => service.DeleteSaveFile(save));
            Directory.Delete(tempDir, true);
            Directory.Delete(backupDir, true);
        }
    }
}
