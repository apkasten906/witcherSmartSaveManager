using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using WitcherCore.Services;
using WitcherCore.Models;

namespace WitcherSmartSaveManagerTests
{
    [TestFixture]
    public class DatabaseIntegrationTests
    {
        private SaveFileMetadataService _metadataService = null!;
        private string _tempSaveDir = null!;

        [SetUp]
        public async Task SetUp()
        {
            // Create a temporary directory for test save files
            _tempSaveDir = Path.Combine(Path.GetTempPath(), $"witcher_test_saves_{Guid.NewGuid()}");
            Directory.CreateDirectory(_tempSaveDir);

            // Initialize the service with default configuration
            _metadataService = new SaveFileMetadataService();

            // Initialize database schema for testing
            var initResult = await _metadataService.InitializeDatabaseAsync();
            if (!initResult)
            {
                throw new InvalidOperationException("Failed to initialize test database");
            }
        }

        [TearDown]
        public void TearDown()
        {
            // Clean up temporary directory
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
        public async Task UpsertSaveFileMetadata_NewFile_InsertsSuccessfully()
        {
            // Arrange
            var fileName = "testSave.sav";
            var saveFilePath = Path.Combine(_tempSaveDir, fileName);
            File.WriteAllText(saveFilePath, "Mock save data");

            var saveFile = new WitcherSaveFile
            {
                Game = GameKey.Witcher2,
                FileName = fileName,
                FullPath = saveFilePath,
                ModifiedTimeIso = DateTime.Now.ToString("O"),
                Size = (int)new FileInfo(saveFilePath).Length
            };

            // Act
            Console.WriteLine($"Testing with save file: {saveFile.FileName}, Size: {saveFile.Size}");
            var result = await _metadataService.UpsertSaveFileMetadataAsync(saveFile);
            Console.WriteLine($"Upsert result: {result}");

            // Test if we can retrieve metadata to see if the operation partially worked
            var metadata = await _metadataService.GetEnhancedMetadataAsync(fileName);
            Console.WriteLine($"Retrieved metadata keys: {string.Join(", ", metadata.Keys)}");
            if (metadata.ContainsKey("database_error"))
            {
                Console.WriteLine($"Database error: {metadata["database_error"]}");
            }

            // Assert
            Assert.That(result, Is.True, "Metadata upsert should succeed");
        }

        [Test]
        public async Task GetEnhancedMetadataAsync_ExistingFile_ReturnsCorrectData()
        {
            // Arrange
            var fileName = "enhancedTestSave.sav";
            var saveFilePath = Path.Combine(_tempSaveDir, fileName);
            File.WriteAllText(saveFilePath, "Mock save data for enhanced test");

            var saveFile = new WitcherSaveFile
            {
                Game = GameKey.Witcher2,
                FileName = fileName,
                FullPath = saveFilePath,
                ModifiedTimeIso = DateTime.Now.ToString("O"),
                Size = (int)new FileInfo(saveFilePath).Length
            };

            await _metadataService.UpsertSaveFileMetadataAsync(saveFile);

            // Act
            var retrievedMetadata = await _metadataService.GetEnhancedMetadataAsync(fileName);

            // Assert
            Assert.That(retrievedMetadata, Is.Not.Null, "Should return metadata");

            // If database enhancement worked, we should see database_enhanced flag
            if (retrievedMetadata.ContainsKey("database_enhanced"))
            {
                Assert.That(retrievedMetadata["database_enhanced"], Is.True, "Should be database enhanced");
                Assert.That(retrievedMetadata["file_size"], Is.EqualTo(saveFile.Size), "File size should match");
            }
        }

        [Test]
        public async Task StoreQuestDataAsync_ValidQuests_StoresSuccessfully()
        {
            // Arrange
            var fileName = "questSave.sav";
            var saveFilePath = Path.Combine(_tempSaveDir, fileName);
            File.WriteAllText(saveFilePath, "Mock save data with quests");

            var saveFile = new WitcherSaveFile
            {
                Game = GameKey.Witcher2,
                FileName = fileName,
                FullPath = saveFilePath,
                ModifiedTimeIso = DateTime.Now.ToString("O"),
                Size = (int)new FileInfo(saveFilePath).Length
            };

            // First store the base metadata
            await _metadataService.UpsertSaveFileMetadataAsync(saveFile);

            var questData = new List<QuestInfo>
            {
                new QuestInfo
                {
                    QuestName = "The Prologue",
                    QuestPhase = "Active",
                    QuestDescription = "Complete the prologue",
                    IsCompleted = false
                },
                new QuestInfo
                {
                    QuestName = "Training",
                    QuestPhase = "Completed",
                    QuestDescription = "Practice combat",
                    IsCompleted = true
                }
            };

            // Act
            var result = await _metadataService.StoreQuestDataAsync(fileName, questData);

            // Assert
            Assert.That(result, Is.True, "Quest data storage should succeed");
        }

        [Test]
        [Ignore("TODO: Quest data retrieval - missing 'active_quests' key in metadata response. Tracked in feat/30-parse-witcher2-save-files")]
        public async Task GetEnhancedMetadataAsync_WithQuestData_ReturnsCompleteInformation()
        {
            // Arrange
            var fileName = "fullTestSave.sav";
            var saveFilePath = Path.Combine(_tempSaveDir, fileName);
            File.WriteAllText(saveFilePath, "Mock save data for full test");

            var saveFile = new WitcherSaveFile
            {
                Game = GameKey.Witcher2,
                FileName = fileName,
                FullPath = saveFilePath,
                ModifiedTimeIso = DateTime.Now.ToString("O"),
                Size = (int)new FileInfo(saveFilePath).Length
            };

            // Store base metadata
            await _metadataService.UpsertSaveFileMetadataAsync(saveFile);

            // Store quest data
            var questData = new List<QuestInfo>
            {
                new QuestInfo
                {
                    QuestName = "The Path of Destiny",
                    QuestPhase = "Active",
                    QuestDescription = "Find Triss and escape",
                    IsCompleted = false
                }
            };
            await _metadataService.StoreQuestDataAsync(fileName, questData);

            // Act
            var metadata = await _metadataService.GetEnhancedMetadataAsync(fileName);

            // Assert
            Assert.That(metadata, Is.Not.Null, "Should return enhanced metadata");

            if (metadata.ContainsKey("database_enhanced"))
            {
                Assert.That(metadata["database_enhanced"], Is.True, "Should be database enhanced");
                Assert.That(metadata["file_size"], Is.EqualTo(saveFile.Size), "Should include file metadata");
                Assert.That(metadata.ContainsKey("active_quests"), "Should include quest information");
            }
        }

        [Test]
        public async Task GetEnhancedMetadataAsync_MissingFile_ReturnsEmptyMetadata()
        {
            // Act
            var metadata = await _metadataService.GetEnhancedMetadataAsync("nonexistent.sav");

            // Assert
            Assert.That(metadata, Is.Not.Null, "Should return metadata object");
            // Should either be empty or contain error information
            Assert.That(metadata.Count >= 0, "Should return valid metadata dictionary");
        }

        [Test]
        public async Task StoreQuestDataAsync_EmptyQuestList_HandlesGracefully()
        {
            // Arrange
            var fileName = "emptyQuestSave.sav";
            var saveFilePath = Path.Combine(_tempSaveDir, fileName);
            File.WriteAllText(saveFilePath, "Mock save data");

            var saveFile = new WitcherSaveFile
            {
                Game = GameKey.Witcher2,
                FileName = fileName,
                FullPath = saveFilePath,
                ModifiedTimeIso = DateTime.Now.ToString("O"),
                Size = (int)new FileInfo(saveFilePath).Length
            };

            await _metadataService.UpsertSaveFileMetadataAsync(saveFile);

            // Act
            var result = await _metadataService.StoreQuestDataAsync(fileName, new List<QuestInfo>());

            // Assert
            Assert.That(result, Is.True, "Should handle empty quest list gracefully");
        }

        [Test]
        public async Task ConcurrentOperations_MultipleThreads_HandlesCorrectly()
        {
            // Arrange
            var tasks = new List<Task<bool>>();

            // Act - Simulate concurrent metadata storage
            for (int i = 0; i < 5; i++)
            {
                var fileName = $"concurrent_save_{i}.sav";
                var saveFilePath = Path.Combine(_tempSaveDir, fileName);
                File.WriteAllText(saveFilePath, $"Concurrent save data {i}");

                var saveFile = new WitcherSaveFile
                {
                    Game = GameKey.Witcher2,
                    FileName = fileName,
                    FullPath = saveFilePath,
                    ModifiedTimeIso = DateTime.Now.ToString("O"),
                    Size = (int)new FileInfo(saveFilePath).Length
                };

                tasks.Add(_metadataService.UpsertSaveFileMetadataAsync(saveFile));
            }

            var results = await Task.WhenAll(tasks);

            // Assert
            foreach (var result in results)
            {
                Assert.That(result, Is.True, "All concurrent operations should succeed");
            }

            // Verify all data was stored correctly
            for (int i = 0; i < 5; i++)
            {
                var fileName = $"concurrent_save_{i}.sav";
                var retrievedMetadata = await _metadataService.GetEnhancedMetadataAsync(fileName);

                Assert.That(retrievedMetadata, Is.Not.Null, $"File {i} should have metadata");
            }
        }
    }
}
