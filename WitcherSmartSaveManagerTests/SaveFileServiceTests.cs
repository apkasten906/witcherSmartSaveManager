using System.IO;
using NUnit.Framework;
using WitcherSmartSaveManager.Tests.Mocks;
using WitcherSmartSaveManager.Utils;

namespace WitcherSmartSaveManager.Tests
{
    [TestFixture]
    public class SaveFileServiceTests
    {
        [TestCase("Witcher1", "*.TheWitcherSave")]
        [TestCase("Witcher2", "*.sav")]
        [TestCase("Witcher3", "*.sav")]
        public void Should_Find_Mocked_Saves_And_Screenshots(string gameKey, string extension)
        {
            var mockPath = TestHelpers.CreateFakeSaveDirectory(gameKey, extension);
            var service = new MockSaveFileService(gameKey, mockPath, extension);

            var saves = service.GetSaveFiles();

            Assert.That(saves.Count, Is.EqualTo(3));
            foreach (var save in saves)
            {
                Assert.That(save.FileName.EndsWith(extension.Replace("*", "")), Is.True);
                Assert.That(File.Exists(save.FullPath), Is.True);
                Assert.That(File.Exists(save.ScreenshotPath), Is.True);
            }
        }
    }

    [TestFixture]
    public class MetadataTests
    {
        [Test]
        public void Should_Populate_Metadata_From_MetadataExtractor()
        {
            var dummyPath = "fake_path.sav";
            var metadata = MetadataExtractor.GetMetadata(dummyPath);

            Assert.That(metadata, Is.Not.Null);
            Assert.That(metadata.ContainsKey("source"), Is.True);
            Assert.That(metadata.ContainsKey("quest"), Is.True);
            Assert.That(metadata["source"], Is.EqualTo("mock"));
        }
    }
}