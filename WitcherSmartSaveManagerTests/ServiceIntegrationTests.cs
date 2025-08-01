using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using WitcherCore.Services;
using WitcherCore.Models;

namespace WitcherSmartSaveManagerTests
{
    [TestFixture]
    public class ServiceIntegrationTests
    {
        private string _tempSaveDir = null!;
        private WitcherSaveFileService _saveFileService = null!;
        private SaveFileMetadataService _metadataService = null!;

        [SetUp]
        public void SetUp()
        {
            // Create temporary directories
            _tempSaveDir = Path.Combine(Path.GetTempPath(), $"witcher_saves_{Guid.NewGuid()}");
            var tempBackupDir = Path.Combine(Path.GetTempPath(), $"witcher_backups_{Guid.NewGuid()}");

            Directory.CreateDirectory(_tempSaveDir);
            Directory.CreateDirectory(tempBackupDir);

            // Initialize services
            _metadataService = new SaveFileMetadataService();
            _saveFileService = new WitcherSaveFileService(GameKey.Witcher2, _tempSaveDir, tempBackupDir);
        }

        [TearDown]
        public void TearDown()
        {
            // Clean up temporary directories
            if (Directory.Exists(_tempSaveDir))
            {
                try
                {
                    Directory.Delete(_tempSaveDir, true);
                }
                catch (IOException)
                {
                    // Files might be locked, ignore for cleanup
                }
            }
        }

        [Test]
        public void GetSaveFiles_WithoutDatabase_ReturnsFileBasedData()
        {
            // Arrange - Create test save files
            var saveFile1 = Path.Combine(_tempSaveDir, "manual_save_1.sav");
            var saveFile2 = Path.Combine(_tempSaveDir, "auto_save_2.sav");

            File.WriteAllText(saveFile1, "Mock save data 1");
            File.WriteAllText(saveFile2, "Mock save data 2");

            // Act
            var saveFiles = _saveFileService.GetSaveFiles();

            // Assert
            Assert.That(saveFiles.Count, Is.EqualTo(2), "Should find both save files");
            Assert.That(saveFiles.Any(s => s.FileName == "manual_save_1.sav"), Is.True, "Should include first save");
            Assert.That(saveFiles.Any(s => s.FileName == "auto_save_2.sav"), Is.True, "Should include second save");

            // Verify file-based metadata is available
            var firstSave = saveFiles.First(s => s.FileName == "manual_save_1.sav");
            Assert.That(firstSave.Size, Is.GreaterThan(0), "Should have file size");
            Assert.That(firstSave.ModifiedTimeIso, Is.Not.Null.And.Not.Empty, "Should have modified time");
        }

        [Test]
        public async Task DatabaseMetadataIntegration_StoreAndRetrieve_WorksCorrectly()
        {
            // Arrange - Create a save file
            var saveFileName = "enhanced_save.sav";
            var saveFilePath = Path.Combine(_tempSaveDir, saveFileName);
            File.WriteAllText(saveFilePath, "Mock save data with metadata");

            var saveFile = new WitcherSaveFile
            {
                Game = GameKey.Witcher2,
                FileName = saveFileName,
                FullPath = saveFilePath,
                ModifiedTimeIso = DateTime.Now.ToString("O"),
                Size = (int)new FileInfo(saveFilePath).Length
            };

            // Act - Store metadata in database
            var storeResult = await _metadataService.UpsertSaveFileMetadataAsync(saveFile);

            // Store some quest data
            var questData = new List<QuestInfo>
            {
                new QuestInfo
                {
                    QuestName = "The Prologue",
                    QuestPhase = "In Progress",
                    QuestDescription = "Complete the tutorial",
                    IsCompleted = false
                }
            };
            var questResult = await _metadataService.StoreQuestDataAsync(saveFileName, questData);

            // Retrieve enhanced metadata
            var enhancedMetadata = await _metadataService.GetEnhancedMetadataAsync(saveFileName);

            // Assert
            Assert.That(storeResult, Is.True, "Should store metadata successfully");
            Assert.That(questResult, Is.True, "Should store quest data successfully");
            Assert.That(enhancedMetadata, Is.Not.Null, "Should retrieve metadata");

            if (enhancedMetadata.ContainsKey("database_enhanced"))
            {
                Assert.That(enhancedMetadata["database_enhanced"], Is.True, "Should be marked as database enhanced");
                Assert.That(enhancedMetadata["file_size"], Is.EqualTo(saveFile.Size), "Should include correct file size");
            }
        }

        [Test]
        public void SaveFileOperations_BasicFileOperations_WorkWithoutDatabase()
        {
            // Arrange - Create a save file
            var saveFileName = "operation_test.sav";
            var saveFilePath = Path.Combine(_tempSaveDir, saveFileName);
            File.WriteAllText(saveFilePath, "Original save data");

            // Act - Get save files (should work without database)
            var saveFiles = _saveFileService.GetSaveFiles();
            var targetSave = saveFiles.FirstOrDefault(s => s.FileName == saveFileName);

            // Assert
            Assert.That(saveFiles.Count, Is.GreaterThan(0), "Should find save files");
            Assert.That(targetSave, Is.Not.Null, "Should find the test save file");
            Assert.That(targetSave!.FileName, Is.EqualTo(saveFileName), "Should have correct filename");
            Assert.That(targetSave.Size, Is.GreaterThan(0), "Should have file size");
            Assert.That(File.Exists(targetSave.FullPath), Is.True, "Save file should exist on disk");
        }

        [Test]
        public async Task DatabaseFallback_ContinuesWorkingWithoutDatabase()
        {
            // Arrange - Create save files
            var saveFile1 = Path.Combine(_tempSaveDir, "fallback_save_1.sav");
            var saveFile2 = Path.Combine(_tempSaveDir, "fallback_save_2.sav");

            File.WriteAllText(saveFile1, "Mock save data 1");
            File.WriteAllText(saveFile2, "Mock save data 2");

            // Act - Operations should still work even if database has issues
            var saveFiles = _saveFileService.GetSaveFiles();

            // Test metadata retrieval (should return empty or default metadata gracefully)
            var metadata = await _metadataService.GetEnhancedMetadataAsync("fallback_save_1.sav");

            // Assert
            Assert.That(saveFiles.Count, Is.EqualTo(2), "Should still find save files");
            Assert.That(metadata, Is.Not.Null, "Should return metadata object (even if empty)");

            // All saves should have basic file information
            foreach (var save in saveFiles)
            {
                Assert.That(save.FileName, Is.Not.Null.And.Not.Empty, "Should have filename");
                Assert.That(save.FullPath, Is.Not.Null.And.Not.Empty, "Should have full path");
                Assert.That(save.Size, Is.GreaterThan(0), "Should have file size");
            }
        }

        [Test]
        public async Task PerformanceTest_ManyFiles_HandlesEfficiently()
        {
            // Arrange - Create multiple save files
            const int fileCount = 25;
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            for (int i = 0; i < fileCount; i++)
            {
                var saveFile = Path.Combine(_tempSaveDir, $"perf_save_{i:D3}.sav");
                File.WriteAllText(saveFile, $"Performance test save data {i}");
            }

            // Act
            var saveFiles = _saveFileService.GetSaveFiles();
            stopwatch.Stop();

            // Assert
            Assert.That(saveFiles.Count, Is.EqualTo(fileCount), $"Should find all {fileCount} save files");
            Assert.That(stopwatch.ElapsedMilliseconds, Is.LessThan(3000),
                $"Should load {fileCount} files quickly, took {stopwatch.ElapsedMilliseconds}ms");

            // Verify all files have basic metadata
            foreach (var save in saveFiles)
            {
                Assert.That(save.FileName, Does.StartWith("perf_save_"), "Should have correct filename pattern");
                Assert.That(save.Size, Is.GreaterThan(0), "Should have file size");
                Assert.That(File.Exists(save.FullPath), Is.True, "File should exist");
            }
        }
    }
}
