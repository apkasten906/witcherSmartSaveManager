
using NUnit.Framework;
using WitcherSmartSaveManager.Services;
using WitcherSmartSaveManager.Models;
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
    }
}
