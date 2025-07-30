using System;
using System.IO;

namespace WitcherSmartSaveManager.Tests
{
    public static class TestHelpers
    {
        public static string CreateFakeSaveDirectory(string gameKey, string extension, int fileCount = 3)
        {
            string tempDir = Path.Combine(Path.GetTempPath(), $"witcher_{gameKey.ToLower()}_test");
            Directory.CreateDirectory(tempDir);

            string ext = extension.Replace("*", "");

            for (int i = 0; i < fileCount; i++)
            {
                string baseName = $"Save{i}";
                string savePath = Path.Combine(tempDir, baseName + ext);
                File.WriteAllText(savePath, $"Fake data for save {i}");

                // Create matching .bmp screenshot
                string bmpPath = Path.Combine(tempDir, baseName + ".bmp");
                File.WriteAllBytes(bmpPath, new byte[] { 66, 77, 0, 0 }); // Simple fake BMP header (BM...)
            }

            return tempDir;
        }
    }
}
