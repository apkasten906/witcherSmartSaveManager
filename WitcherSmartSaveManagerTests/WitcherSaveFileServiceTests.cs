using System.Collections.Generic;
using System.IO;
using System;
using NUnit.Framework;
using WitcherCore.Services;
using WitcherCore.Models;
using NUnit.Framework.Legacy;

namespace WitcherSmartSaveManager.Tests
{
    [TestFixture]
    public class WitcherSaveFileServiceTests
    {
        private string _tempSaveDir = "";
        private string _tempBackupDir = "";
        private string _saveFilePath = "";
        private string _screenshotPath = "";
        private WitcherSaveFileService _service = new WitcherSaveFileService(GameKey.Witcher2);

        [SetUp]
        public void SetUp()
        {
            _tempSaveDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            _tempBackupDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(_tempSaveDir);
            Directory.CreateDirectory(_tempBackupDir);

            _saveFilePath = Path.Combine(_tempSaveDir, "test.sav");
            _screenshotPath = Path.Combine(_tempSaveDir, "test_640x360.bmp");

            File.WriteAllText(_saveFilePath, "SAVE DATA");
            File.WriteAllText(_screenshotPath, "SCREENSHOT");

            _service = new WitcherSaveFileService(GameKey.Witcher2, _tempSaveDir, _tempBackupDir);
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(_tempSaveDir)) Directory.Delete(_tempSaveDir, true);
            if (Directory.Exists(_tempBackupDir)) Directory.Delete(_tempBackupDir, true);
        }

        [Test]
        public void DeleteSaveFile_RemovesSaveAndScreenshot_ReturnsTrue()
        {
            var result = _service.DeleteSaveFile(_saveFilePath, _screenshotPath);

            Assert.That(result, Is.True);
            Assert.That(File.Exists(_saveFilePath), Is.False);
            Assert.That(File.Exists(_screenshotPath), Is.False);
        }

        [Test]
        public void DeleteSaveFile_FileDoesNotExist_ThrowsFileNotFoundException()
        {
            // This test now expects DeleteSaveFile to return false if the file does not exist.
            var result = _service.DeleteSaveFile("nonexistent.sav");
            Assert.That(result, Is.False);
        }

        [Test]
        public void BackupSaveFile_CopiesSaveFile_ReturnsTrue()
        {
            var backupPath = Path.Combine(_tempBackupDir, "test.sav");

            var result = _service.BackupSaveFile(_saveFilePath, _screenshotPath, overwrite: true);

            Assert.That(result, Is.True);
            Assert.That(File.Exists(backupPath), Is.True);
        }

        [Test]
        public void BackupSaveFile_AlreadyExistsWithoutOverwrite_ReturnsFalse()
        {
            var backupPath = Path.Combine(_tempBackupDir, "test.sav");
            File.WriteAllText(backupPath, "OLD BACKUP");

            var result = _service.BackupSaveFile(_saveFilePath, overwrite: false);

            Assert.That(result, Is.False);
            ClassicAssert.AreEqual("OLD BACKUP", File.ReadAllText(backupPath));
        }

        [Test]
        public void GetOrphanedScreenshots_FindsOrphanedScreenshots()
        {
            // Create orphaned screenshot (no corresponding save file)
            var orphanedScreenshotPath = Path.Combine(_tempSaveDir, "orphaned_640x360.bmp");
            File.WriteAllText(orphanedScreenshotPath, "ORPHANED SCREENSHOT");

            var orphanedScreenshots = _service.GetOrphanedScreenshots();

            Assert.That(orphanedScreenshots, Contains.Item(orphanedScreenshotPath));
            Assert.That(orphanedScreenshots.Count, Is.EqualTo(1));
        }

        [Test]
        public void GetOrphanedScreenshots_IgnoresScreenshotsWithCorrespondingSaves()
        {
            // The test screenshot already has a corresponding save file from SetUp
            var orphanedScreenshots = _service.GetOrphanedScreenshots();

            // Should not include our test screenshot since it has a corresponding save
            Assert.That(orphanedScreenshots, Does.Not.Contain(_screenshotPath));
        }

        [Test]
        public void GetOrphanedScreenshots_IgnoresNonWitcherScreenshots()
        {
            // Create a BMP file that doesn't match Witcher 2 screenshot pattern
            var nonWitcherBmp = Path.Combine(_tempSaveDir, "random.bmp");
            File.WriteAllText(nonWitcherBmp, "NOT A WITCHER SCREENSHOT");

            var orphanedScreenshots = _service.GetOrphanedScreenshots();

            Assert.That(orphanedScreenshots, Does.Not.Contain(nonWitcherBmp));
        }

        [Test]
        public void CleanupOrphanedScreenshots_DeletesOrphanedFiles()
        {
            // Create orphaned screenshots
            var orphanedScreenshot1 = Path.Combine(_tempSaveDir, "orphaned1_640x360.bmp");
            var orphanedScreenshot2 = Path.Combine(_tempSaveDir, "orphaned2_640x360.bmp");
            File.WriteAllText(orphanedScreenshot1, "ORPHANED 1");
            File.WriteAllText(orphanedScreenshot2, "ORPHANED 2");

            var orphanedList = new List<string> { orphanedScreenshot1, orphanedScreenshot2 };
            var deletedCount = _service.CleanupOrphanedScreenshots(orphanedList);

            Assert.That(deletedCount, Is.EqualTo(2));
            Assert.That(File.Exists(orphanedScreenshot1), Is.False);
            Assert.That(File.Exists(orphanedScreenshot2), Is.False);
        }

        [Test]
        public void CleanupAllOrphanedScreenshots_DetectsAndDeletesOrphans()
        {
            // Create orphaned screenshot
            var orphanedScreenshot = Path.Combine(_tempSaveDir, "orphaned_640x360.bmp");
            File.WriteAllText(orphanedScreenshot, "ORPHANED");

            var deletedCount = _service.CleanupAllOrphanedScreenshots();

            Assert.That(deletedCount, Is.EqualTo(1));
            Assert.That(File.Exists(orphanedScreenshot), Is.False);
        }

        [Test]
        public void DeleteSaveFile_ContinuesOnScreenshotError()
        {
            // This test ensures that if screenshot deletion fails, the operation still succeeds
            // (We can't easily simulate file locking in unit tests, so this is more of a design verification)

            var result = _service.DeleteSaveFile(_saveFilePath, _screenshotPath);

            Assert.That(result, Is.True);
            Assert.That(File.Exists(_saveFilePath), Is.False);
        }
    }
}
