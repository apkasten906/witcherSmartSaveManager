using System;
using System.IO;
using System.Linq;
using WitcherCore.Services;
using NLog;

namespace WitcherCore.Tools
{
    /// <summary>
    /// Console tool for analyzing Witcher 2 save files
    /// Usage: Run this to discover the save file format structure
    /// </summary>
    public class SaveAnalysisTool
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly SaveFileHexAnalyzer _analyzer;

        public SaveAnalysisTool()
        {
            _analyzer = new SaveFileHexAnalyzer();
        }

        /// <summary>
        /// Run analysis on all save files in the savesAnalysis\_backup directory
        /// </summary>
        public void AnalyzeBackupSaves()
        {
            var backupDir = Path.Combine(Directory.GetCurrentDirectory(), "savesAnalysis", "_backup");

            if (!Directory.Exists(backupDir))
            {
                Console.WriteLine($"❌ Backup directory not found: {backupDir}");
                Console.WriteLine("Please ensure your save files are in the savesAnalysis\\_backup directory");
                return;
            }

            Console.WriteLine($"🔍 Analyzing Witcher 2 save files in: {backupDir}");
            Console.WriteLine();

            // Find all .sav files
            var saveFiles = Directory.GetFiles(backupDir, "*.sav", SearchOption.AllDirectories)
                                   .OrderBy(f => new FileInfo(f).LastWriteTime)
                                   .ToList();

            if (saveFiles.Count == 0)
            {
                Console.WriteLine("❌ No .sav files found in the backup directory");
                return;
            }

            Console.WriteLine($"📁 Found {saveFiles.Count} save files:");
            foreach (var file in saveFiles)
            {
                var info = new FileInfo(file);
                Console.WriteLine($"   {Path.GetFileName(file)} ({info.Length:N0} bytes, {info.LastWriteTime:yyyy-MM-dd HH:mm})");
            }
            Console.WriteLine();

            // Analyze individual files first
            Console.WriteLine("🔬 Individual File Analysis:");
            Console.WriteLine("=" + new string('=', 50));

            var analyses = saveFiles.Take(3) // Analyze first 3 files in detail
                                   .Select(file => _analyzer.AnalyzeSaveFile(file))
                                   .Where(result => result.Success)
                                   .ToList();

            foreach (var analysis in analyses)
            {
                PrintFileAnalysis(analysis);
            }

            // Compare files to find patterns
            if (saveFiles.Count >= 2)
            {
                Console.WriteLine();
                Console.WriteLine("🔍 Comparative Analysis:");
                Console.WriteLine("=" + new string('=', 50));

                var comparison = _analyzer.CompareSaveFiles(saveFiles.Take(5).ToList()); // Compare first 5 files
                if (comparison.Success)
                {
                    PrintComparisonResults(comparison);
                }
                else
                {
                    Console.WriteLine($"❌ Comparison failed: {comparison.ErrorMessage}");
                }
            }

            Console.WriteLine();
            Console.WriteLine("✅ Analysis complete! Use these findings to implement the real parser.");
            Console.WriteLine("📝 Look for patterns in static offsets for file structure,");
            Console.WriteLine("   and variable offsets for quest progress, player stats, etc.");
        }

        /// <summary>
        /// Analyze a specific save file by path
        /// </summary>
        public void AnalyzeSpecificFile(string filePath)
        {
            Console.WriteLine($"🔍 Analyzing specific file: {filePath}");
            Console.WriteLine();

            var analysis = _analyzer.AnalyzeSaveFile(filePath);
            if (analysis.Success)
            {
                PrintFileAnalysis(analysis);
            }
            else
            {
                Console.WriteLine($"❌ Analysis failed: {analysis.ErrorMessage}");
            }
        }

        private void PrintFileAnalysis(Services.HexAnalysisResult analysis)
        {
            Console.WriteLine($"📄 {analysis.FileName}");
            Console.WriteLine($"   Size: {analysis.FileSize:N0} bytes");
            Console.WriteLine($"   Modified: {analysis.LastModified:yyyy-MM-dd HH:mm:ss}");
            Console.WriteLine();

            if (analysis.PossibleMagicBytes.Any())
            {
                Console.WriteLine("🔮 Possible Magic Bytes/Signatures:");
                foreach (var magic in analysis.PossibleMagicBytes.Take(5))
                {
                    Console.WriteLine($"   {magic}");
                }
                Console.WriteLine();
            }

            if (analysis.PossibleStrings.Any())
            {
                Console.WriteLine("📝 Readable Strings Found:");
                foreach (var str in analysis.PossibleStrings.Take(10))
                {
                    Console.WriteLine($"   {str}");
                }
                Console.WriteLine();
            }

            if (analysis.SuspiciousOffsets.Any())
            {
                Console.WriteLine("🎯 Suspicious Data (timestamps, stats, money):");
                foreach (var offset in analysis.SuspiciousOffsets.Take(8))
                {
                    Console.WriteLine($"   {offset}");
                }
                Console.WriteLine();
            }

            if (analysis.RepeatingPatterns.Any())
            {
                Console.WriteLine("🔄 Repeating Patterns:");
                foreach (var pattern in analysis.RepeatingPatterns.Take(5))
                {
                    Console.WriteLine($"   {pattern}");
                }
                Console.WriteLine();
            }

            // Show first few lines of hex dump
            var hexLines = analysis.HexDump.Split('\n').Take(10);
            Console.WriteLine("🔢 Hex Dump (first 10 lines):");
            foreach (var line in hexLines)
            {
                if (!string.IsNullOrWhiteSpace(line))
                    Console.WriteLine($"   {line}");
            }

            Console.WriteLine();
            Console.WriteLine("-" + new string('-', 70));
            Console.WriteLine();
        }

        private void PrintComparisonResults(Services.HexComparisonResult comparison)
        {
            Console.WriteLine(comparison.DifferenceReport);
            Console.WriteLine();

            if (comparison.CommonStrings.Any())
            {
                Console.WriteLine("🔗 Common Strings (appear in all files):");
                foreach (var str in comparison.CommonStrings.Take(10))
                {
                    Console.WriteLine($"   {str}");
                }
                Console.WriteLine();
            }

            if (comparison.StaticOffsets.Any())
            {
                Console.WriteLine("🏗️ Static Structure (same in all files - first 20 bytes):");
                foreach (var offset in comparison.StaticOffsets.Take(20))
                {
                    Console.WriteLine($"   {offset}");
                }
                Console.WriteLine();
            }

            if (comparison.VariableOffsets.Any())
            {
                Console.WriteLine("🎲 Variable Data (changes between files - first 20):");
                foreach (var offset in comparison.VariableOffsets.Take(20))
                {
                    Console.WriteLine($"   {offset}");
                }
                Console.WriteLine();
            }
        }
    }
}
