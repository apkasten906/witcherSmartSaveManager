using System;
using System.Collections.Generic;
using System.IO;
using WitcherGuiApp.Models;

namespace WitcherGuiApp.Tests.Mocks
{
    public class MockSaveFileService
    {
        private readonly string _mockPath;
        private readonly string _mockExtension;

        public MockSaveFileService(string gameKey, string mockPath, string mockExtension)
        {
            _mockPath = mockPath;
            _mockExtension = mockExtension;
        }

        public List<WitcherSaveFile> GetSaveFiles()
        {
            if (!Directory.Exists(_mockPath))
                return new List<WitcherSaveFile>();

            var files = Directory.EnumerateFiles(_mockPath, _mockExtension, SearchOption.TopDirectoryOnly);


            // TODO: fix this and make it easier to implement
            var result = new List<WitcherSaveFile>();
            foreach (var file in files)
            {
                var info = new FileInfo(file);
                string baseName = Path.GetFileNameWithoutExtension(info.Name);
                string bmpPath = Path.Combine(_mockPath, baseName + ".bmp");

                result.Add(new WitcherSaveFile
                {
                    Game = GameKey.Witcher2,
                    FileName = info.Name,
                    ModifiedTime = new DateTimeOffset(info.LastWriteTimeUtc).ToUnixTimeSeconds(),
                    ModifiedTimeIso = info.LastWriteTimeUtc.ToString("o"),
                    Size = (int)info.Length,
                    FullPath = info.FullName,
                    ScreenshotPath = File.Exists(bmpPath) ? bmpPath : "",
                    Metadata = new Dictionary<string, object>()
                });
            }

            return result;
        }
    }
}
