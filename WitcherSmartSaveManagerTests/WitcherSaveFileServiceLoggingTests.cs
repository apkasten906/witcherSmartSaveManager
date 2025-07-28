using NUnit.Framework;
using WitcherSmartSaveManager.Services;
using WitcherSmartSaveManager.Models;
using System.IO;
using System;

namespace WitcherSmartSaveManager.Tests
{
    [TestFixture]
    public class WitcherSaveFileServiceLoggingTests
    {
        [Test]
        public void GetSaveFiles_LogsWarning_WhenDirectoryMissing()
        {
            var service = new WitcherSaveFileService(GameKey.Witcher2, "Z:\\NonExistentPath", "Z:\\BackupPath");
            var saves = service.GetSaveFiles();
            Assert.That(saves, Is.Empty);
            // Manual: Check logs/app.log for warning about missing directory
        }

        [Test]
        public void BackupSaveFile_Throws_WhenFileMissing()
        {
            var service = new WitcherSaveFileService(GameKey.Witcher2, Path.GetTempPath(), Path.GetTempPath());
            var dummySave = new WitcherSaveFile { FullPath = "Z:\\NonExistentFile.sav", ScreenshotPath = "" };
            Assert.Throws<FileNotFoundException>(() => service.BackupSaveFile(dummySave));
            // Manual: Check logs/app.log for error about missing file
        }
    }
}
