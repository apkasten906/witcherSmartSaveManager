using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using WitcherSmartSaveManager.Services;
using WitcherSmartSaveManager.Models;

namespace WitcherSmartSaveManager.Tests
{
    [TestFixture]
    public class OrphanedScreenshotTests
    {
        private string _tempSaveDir = "";
        private string _tempBackupDir = "";
        private WitcherSaveFileService _service = null!;

        [SetUp]
        public void SetUp()
        {
            _tempSaveDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            _tempBackupDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(_tempSaveDir);
            Directory.CreateDirectory(_tempBackupDir);

            _service = new WitcherSaveFileService(GameKey.Witcher2, _tempSaveDir, _tempBackupDir);
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(_tempSaveDir)) Directory.Delete(_tempSaveDir, true);
            if (Directory.Exists(_tempBackupDir)) Directory.Delete(_tempBackupDir, true);
        }

        [Test]
        public void GetOrphanedScreenshots_WithNonExistentDirectory_ReturnsEmptyList()
        {
            // Delete the save directory to simulate non-existent path
            Directory.Delete(_tempSaveDir, true);

            var orphanedScreenshots = _service.GetOrphanedScreenshots();

            Assert.That(orphanedScreenshots, Is.Not.Null);
            Assert.That(orphanedScreenshots.Count, Is.EqualTo(0));
        }

        [Test]
        public void CleanupOrphanedScreenshots_WithEmptyList_ReturnsZero()
        {
            var emptyList = new List<string>();
            var deletedCount = _service.CleanupOrphanedScreenshots(emptyList);

            Assert.That(deletedCount, Is.EqualTo(0));
        }

        [Test]
        public void CleanupOrphanedScreenshots_WithNullList_ReturnsZero()
        {
            var deletedCount = _service.CleanupOrphanedScreenshots(null);

            Assert.That(deletedCount, Is.EqualTo(0));
        }

        [Test]
        public void GetOrphanedScreenshots_ComplexScenario_WorksCorrectly()
        {
            // Create a save file with its screenshot
            var saveFile = Path.Combine(_tempSaveDir, "save1.sav");
            var saveScreenshot = Path.Combine(_tempSaveDir, "save1_640x360.bmp");
            File.WriteAllText(saveFile, "SAVE DATA");
            File.WriteAllText(saveScreenshot, "SCREENSHOT");

            // Create orphaned screenshots
            var orphaned1 = Path.Combine(_tempSaveDir, "orphaned1_640x360.bmp");
            var orphaned2 = Path.Combine(_tempSaveDir, "orphaned2_640x360.bmp");
            File.WriteAllText(orphaned1, "ORPHANED 1");
            File.WriteAllText(orphaned2, "ORPHANED 2");

            // Create a non-Witcher BMP file
            var nonWitcherBmp = Path.Combine(_tempSaveDir, "random.bmp");
            File.WriteAllText(nonWitcherBmp, "NOT A WITCHER SCREENSHOT");

            var orphanedScreenshots = _service.GetOrphanedScreenshots();

            // Should find only the true orphans, not the one with a corresponding save or the non-Witcher BMP
            Assert.That(orphanedScreenshots.Count, Is.EqualTo(2));
            Assert.That(orphanedScreenshots, Contains.Item(orphaned1));
            Assert.That(orphanedScreenshots, Contains.Item(orphaned2));
            Assert.That(orphanedScreenshots, Does.Not.Contain(saveScreenshot));
            Assert.That(orphanedScreenshots, Does.Not.Contain(nonWitcherBmp));
        }

        [Test]
        public void DeleteSaveFile_FileInUse_HandlesGracefully()
        {
            // Create save and screenshot files
            var saveFile = Path.Combine(_tempSaveDir, "test.sav");
            var screenshot = Path.Combine(_tempSaveDir, "test_640x360.bmp");
            File.WriteAllText(saveFile, "SAVE DATA");
            File.WriteAllText(screenshot, "SCREENSHOT");

            // Test normal deletion (should work fine)
            var result = _service.DeleteSaveFile(saveFile, screenshot);

            Assert.That(result, Is.True);
            Assert.That(File.Exists(saveFile), Is.False);
            Assert.That(File.Exists(screenshot), Is.False);
        }
    }
}
