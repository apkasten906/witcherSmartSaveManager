using NUnit.Framework;
using System;
using System.IO;
using System.Text;
using WitcherCore.Services;
using WitcherCore.Models;

namespace WitcherSmartSaveManagerTests
{
    [TestFixture]
    public class WitcherSaveFileParserTests
    {
        private WitcherSaveFileParser _parser = null!;
        private string _testSaveFilePath = null!;
        private string _tempDir = null!;

        [SetUp]
        public void SetUp()
        {
            _parser = new WitcherSaveFileParser();
            _tempDir = Path.Combine(Path.GetTempPath(), "WitcherParserTests", Guid.NewGuid().ToString());
            Directory.CreateDirectory(_tempDir);
            _testSaveFilePath = Path.Combine(_tempDir, "test_save.sav");
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(_tempDir))
            {
                Directory.Delete(_tempDir, true);
            }
        }

        [Test]
        public void ParseSaveFile_NonExistentFile_ReturnsErrorResult()
        {
            // Arrange
            var nonExistentPath = Path.Combine(_tempDir, "nonexistent.sav");

            // Act
            var result = _parser.ParseSaveFile(nonExistentPath);

            // Assert
            Assert.That(result.ParseResult.Success, Is.False);
            Assert.That(result.ParseResult.ErrorMessage, Does.Contain("not found"));
        }

        [Test]
        public void ParseSaveFile_EmptyFile_HandlesGracefully()
        {
            // Arrange
            File.WriteAllBytes(_testSaveFilePath, new byte[0]);

            // Act
            var result = _parser.ParseSaveFile(_testSaveFilePath);

            // Assert
            Assert.That(result.ParseResult.Success, Is.False);
            Assert.That(result.ParseResult.ErrorMessage, Is.Not.Null);
        }

        [Test]
        public void ParseSaveFile_MockSaveFile_ParsesBasicStructure()
        {
            // Arrange
            CreateMockSaveFile(_testSaveFilePath);

            // Act
            var result = _parser.ParseSaveFile(_testSaveFilePath);

            // Assert
            Assert.That(result.ParseResult.Success, Is.True);
            Assert.That(result.Header, Is.Not.Null);
            Assert.That(result.Quests, Is.Not.Null);
            Assert.That(result.CharacterVariables, Is.Not.Null);
            Assert.That(result.Inventory, Is.Not.Null);
        }

        [Test]
        public void ParseHeader_MockData_ExtractsBasicInfo()
        {
            // Arrange
            CreateMockSaveFile(_testSaveFilePath);

            // Act
            var result = _parser.ParseSaveFile(_testSaveFilePath);

            // Assert
            Assert.That(result.ParseResult.Success, Is.True);
            Assert.That(result.Header.Version, Is.Not.Empty);
            // Note: More specific assertions will be added as we discover the actual format
        }

        [Test]
        public void GetSaveFileSummary_ValidData_ReturnsFormattedSummary()
        {
            // Arrange
            var saveData = new SaveFileData
            {
                Header = new WitcherCore.Models.SaveFileHeader
                {
                    PlayerName = "Geralt",
                    Version = "1.0",
                    Level = 10,
                    PlaytimeMinutes = 120
                },
                ParseResult = new ParseResult { Success = true }
            };

            // Act
            var summary = _parser.GetSaveFileSummary(saveData);

            // Assert
            Assert.That(summary, Does.Contain("Geralt"));
            Assert.That(summary, Does.Contain("Level: 10"));
            Assert.That(summary, Does.Contain("120 minutes"));
        }

        [Test]
        public void GetSaveFileSummary_FailedParse_ReturnsErrorMessage()
        {
            // Arrange
            var saveData = new SaveFileData
            {
                ParseResult = new ParseResult
                {
                    Success = false,
                    ErrorMessage = "Test error"
                }
            };

            // Act
            var summary = _parser.GetSaveFileSummary(saveData);

            // Assert
            Assert.That(summary, Does.Contain("Failed to parse"));
            Assert.That(summary, Does.Contain("Test error"));
        }

        /// <summary>
        /// Create a mock Witcher 2 save file for testing
        /// This will be refined as we discover the actual format in Phase 1
        /// </summary>
        private void CreateMockSaveFile(string filePath)
        {
            using var writer = new BinaryWriter(File.Create(filePath));

            // Mock header structure (will be updated based on Phase 1 discoveries)

            // Version string (16 bytes)
            var versionBytes = new byte[16];
            Encoding.UTF8.GetBytes("WITCHER2_SAVE_V1").CopyTo(versionBytes, 0);
            writer.Write(versionBytes);

            // Padding to timestamp location (0x40)
            writer.Write(new byte[0x40 - 16]);

            // Timestamp (4 bytes)
            var timestamp = (uint)DateTimeOffset.Now.ToUnixTimeSeconds();
            writer.Write(timestamp);

            // Padding to player name location (0x80)
            writer.Write(new byte[0x80 - 0x44]);

            // Player name (64 bytes)
            var nameBytes = new byte[64];
            Encoding.UTF8.GetBytes("TestPlayer").CopyTo(nameBytes, 0);
            writer.Write(nameBytes);

            // Add some mock quest data (structure TBD)
            writer.Write(new byte[1024]); // Mock quest section

            // Add some mock character variables (structure TBD)
            writer.Write(new byte[512]); // Mock variables section
        }

        [Test]
        public void ParseSaveFile_CorruptedData_HandlesGracefully()
        {
            // Arrange
            var corruptData = new byte[100];
            new Random().NextBytes(corruptData);
            File.WriteAllBytes(_testSaveFilePath, corruptData);

            // Act
            var result = _parser.ParseSaveFile(_testSaveFilePath);

            // Assert - Should not crash and should report failure gracefully
            Assert.That(result, Is.Not.Null);
            Assert.That(result.ParseResult, Is.Not.Null);
        }

        [Test]
        public void ParseSaveFile_LargeFile_PerformsReasonably()
        {
            // Arrange
            var largeData = new byte[10 * 1024 * 1024]; // 10MB mock save
            File.WriteAllBytes(_testSaveFilePath, largeData);

            // Act
            var startTime = DateTime.Now;
            var result = _parser.ParseSaveFile(_testSaveFilePath);
            var duration = DateTime.Now - startTime;

            // Assert - Should complete within reasonable time
            Assert.That(duration.TotalSeconds, Is.LessThan(10), "Parsing should complete within 10 seconds");
            Assert.That(result, Is.Not.Null);
        }
    }
}
