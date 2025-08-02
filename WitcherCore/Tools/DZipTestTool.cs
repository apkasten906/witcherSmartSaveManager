using System;
using System.IO;
using System.Linq;
using WitcherCore.Services;

namespace WitcherCore.Tools
{
    /// <summary>
    /// Test program for DZIP decompression - Phase 1.1
    /// Tests our decompressor against real Witcher 2 save files
    /// </summary>
    public class DZipTestTool
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("DZIP Decompression Test Tool - Phase 1.1");
            Console.WriteLine("==========================================");
            Console.WriteLine();

            var backupDir = @".\savesAnalysis\_backup";

            if (!Directory.Exists(backupDir))
            {
                Console.WriteLine($"Error: Backup directory not found: {backupDir}");
                return;
            }

            var saveFiles = Directory.GetFiles(backupDir, "*.sav");
            Console.WriteLine($"Found {saveFiles.Length} save files for decompression testing");

            var decompressor = new DZipDecompressor();

            // Test first 3 files
            var testFiles = saveFiles.Take(3).ToArray();

            foreach (var file in testFiles)
            {
                Console.WriteLine();
                Console.WriteLine($"Testing: {Path.GetFileName(file)}");
                Console.WriteLine(new string('-', 50));

                var result = decompressor.DecompressSaveFile(file);

                if (result.Success)
                {
                    Console.WriteLine($"✓ Decompression successful!");
                    Console.WriteLine($"  Original size: {new FileInfo(file).Length:N0} bytes");
                    Console.WriteLine($"  Uncompressed size: {result.UncompressedSize:N0} bytes");
                    Console.WriteLine($"  Compression ratio: {result.CompressionRatio:F2}");
                    Console.WriteLine($"  Header version: {result.Header?.Version}");
                    Console.WriteLine($"  Compression type: {result.Header?.CompressionType}");

                    // Analyze first few bytes of uncompressed data
                    if (result.UncompressedData != null && result.UncompressedData.Length >= 16)
                    {
                        Console.WriteLine("  First 16 bytes of uncompressed data:");
                        var hexBytes = result.UncompressedData.Take(16)
                            .Select(b => b.ToString("X2"))
                            .ToArray();
                        Console.WriteLine($"  {string.Join(" ", hexBytes)}");
                    }
                }
                else
                {
                    Console.WriteLine($"✗ Decompression failed: {result.ErrorMessage}");
                }
            }

            Console.WriteLine();
            Console.WriteLine("DZIP decompression test complete!");
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }
    }
}
