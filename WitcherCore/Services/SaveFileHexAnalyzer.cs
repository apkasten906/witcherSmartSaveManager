using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using NLog;

namespace WitcherCore.Services
{
    /// <summary>
    /// Hex analysis tool for discovering Witcher 2 save file format
    /// Phase 1: Structure Discovery - helps identify patterns, offsets, and data sections
    /// </summary>
    public class SaveFileHexAnalyzer
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Analyze a single save file and generate detailed hex dump with pattern detection
        /// </summary>
        public HexAnalysisResult AnalyzeSaveFile(string filePath)
        {
            var result = new HexAnalysisResult
            {
                FilePath = filePath,
                FileName = Path.GetFileName(filePath)
            };

            try
            {
                if (!File.Exists(filePath))
                {
                    result.ErrorMessage = $"File not found: {filePath}";
                    return result;
                }

                var fileInfo = new FileInfo(filePath);
                result.FileSize = fileInfo.Length;
                result.LastModified = fileInfo.LastWriteTime;

                Logger.Info($"Analyzing save file: {filePath} ({result.FileSize:N0} bytes)");

                var data = File.ReadAllBytes(filePath);
                result.RawData = data;

                // Generate hex dump for manual inspection
                result.HexDump = GenerateHexDump(data, maxBytes: 2048); // First 2KB for overview

                // Look for common patterns
                result.PossibleMagicBytes = FindMagicBytes(data);
                result.PossibleStrings = FindReadableStrings(data);
                result.SuspiciousOffsets = FindSuspiciousOffsets(data);
                result.RepeatingPatterns = FindRepeatingPatterns(data);

                // Check for DZIP format (discovered in Witcher 2 saves)
                result.DZipHeader = AnalyzeDZipHeader(data);

                result.Success = true;
                Logger.Info($"Analysis complete. Found {result.PossibleStrings.Count} strings, {result.SuspiciousOffsets.Count} suspicious offsets");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"Failed to analyze save file: {filePath}");
                result.ErrorMessage = ex.Message;
            }

            return result;
        }

        /// <summary>
        /// Compare multiple save files to identify what changes between saves
        /// This is crucial for understanding the format
        /// </summary>
        public HexComparisonResult CompareSaveFiles(List<string> filePaths)
        {
            var comparison = new HexComparisonResult();

            try
            {
                Logger.Info($"Comparing {filePaths.Count} save files to identify differences");

                var analyses = new List<HexAnalysisResult>();
                foreach (var path in filePaths)
                {
                    var analysis = AnalyzeSaveFile(path);
                    if (analysis.Success)
                    {
                        analyses.Add(analysis);
                    }
                }

                if (analyses.Count < 2)
                {
                    comparison.ErrorMessage = "Need at least 2 valid save files for comparison";
                    return comparison;
                }

                // Find common strings across all files
                comparison.CommonStrings = FindCommonStrings(analyses);

                // Find bytes that change between files (likely dynamic data)
                comparison.VariableOffsets = FindVariableOffsets(analyses);

                // Find bytes that stay the same (likely structure/headers)
                comparison.StaticOffsets = FindStaticOffsets(analyses);

                // Generate difference report
                comparison.DifferenceReport = GenerateDifferenceReport(analyses);

                comparison.Success = true;
                Logger.Info($"Comparison complete. Found {comparison.CommonStrings.Count} common strings, {comparison.VariableOffsets.Count} variable offsets");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"Failed to compare save files");
                comparison.ErrorMessage = ex.Message;
            }

            return comparison;
        }

        /// <summary>
        /// Generate a formatted hex dump with ASCII representation
        /// </summary>
        private string GenerateHexDump(byte[] data, int maxBytes = int.MaxValue)
        {
            var sb = new StringBuilder();
            var bytesToShow = Math.Min(data.Length, maxBytes);

            sb.AppendLine($"Hex Dump (showing first {bytesToShow:N0} of {data.Length:N0} bytes):");
            sb.AppendLine("Offset    00 01 02 03 04 05 06 07 08 09 0A 0B 0C 0D 0E 0F    ASCII");
            sb.AppendLine("--------  -----------------------------------------------  ----------------");

            for (int i = 0; i < bytesToShow; i += 16)
            {
                sb.Append($"{i:X8}  ");

                // Hex bytes
                for (int j = 0; j < 16; j++)
                {
                    if (i + j < bytesToShow)
                    {
                        sb.Append($"{data[i + j]:X2} ");
                    }
                    else
                    {
                        sb.Append("   ");
                    }
                }

                sb.Append(" ");

                // ASCII representation
                for (int j = 0; j < 16 && i + j < bytesToShow; j++)
                {
                    var b = data[i + j];
                    sb.Append(b >= 32 && b <= 126 ? (char)b : '.');
                }

                sb.AppendLine();
            }

            return sb.ToString();
        }

        /// <summary>
        /// Look for magic bytes or signatures at the beginning of the file
        /// </summary>
        private List<string> FindMagicBytes(byte[] data)
        {
            var magicBytes = new List<string>();

            if (data.Length >= 4)
            {
                // First 4 bytes as hex
                var first4 = data.Take(4).Select(b => b.ToString("X2"));
                magicBytes.Add($"First 4 bytes: {string.Join(" ", first4)}");

                // Check for DZIP magic
                if (data[0] == 0x44 && data[1] == 0x5A && data[2] == 0x49 && data[3] == 0x50)
                {
                    magicBytes.Add("DZIP compression format detected");
                }
            }

            if (data.Length >= 8)
            {
                // First 8 bytes as hex
                var first8 = data.Take(8).Select(b => b.ToString("X2"));
                magicBytes.Add($"First 8 bytes: {string.Join(" ", first8)}");
            }

            // Look for common game save signatures
            var signatures = new[]
            {
                "SAVE", "SAV", "WITCHER", "WIT", "CDProjekt", "CDP", "DZIP",
                new string(new char[] { 'G', 'A', 'M', 'E' }),
                new string(new char[] { 'S', 'A', 'V', 'E' })
            };

            foreach (var sig in signatures)
            {
                var sigBytes = Encoding.UTF8.GetBytes(sig);
                for (int i = 0; i <= Math.Min(256, data.Length - sigBytes.Length); i++)
                {
                    bool match = true;
                    for (int j = 0; j < sigBytes.Length; j++)
                    {
                        if (data[i + j] != sigBytes[j])
                        {
                            match = false;
                            break;
                        }
                    }
                    if (match)
                    {
                        magicBytes.Add($"Found '{sig}' at offset 0x{i:X4}");
                    }
                }
            }

            return magicBytes;
        }

        /// <summary>
        /// Analyze DZIP header format discovered in Witcher 2 save files
        /// Based on Phase 1 analysis findings
        /// </summary>
        private DZipInfo? AnalyzeDZipHeader(byte[] data)
        {
            if (data.Length < 24) return null;

            // Check for DZIP magic bytes
            if (!(data[0] == 0x44 && data[1] == 0x5A && data[2] == 0x49 && data[3] == 0x50))
            {
                return null;
            }

            try
            {
                var dzipInfo = new DZipInfo
                {
                    IsDZipFile = true,
                    Version = BitConverter.ToUInt32(data, 4),
                    CompressionType = BitConverter.ToUInt32(data, 8),
                    DataType = BitConverter.ToUInt32(data, 12),
                    UncompressedSize = BitConverter.ToUInt32(data, 16)
                };

                // Copy compression header (8 bytes at offset 24)
                if (data.Length >= 32)
                {
                    Array.Copy(data, 24, dzipInfo.CompressionHeader, 0, 8);
                }

                // Generate summary
                var sb = new StringBuilder();
                sb.AppendLine("DZIP Compression Format:");
                sb.AppendLine($"  Version/Flags: {dzipInfo.Version}");
                sb.AppendLine($"  Compression Type: {dzipInfo.CompressionType}");
                sb.AppendLine($"  Data Type: {dzipInfo.DataType}");
                sb.AppendLine($"  Uncompressed Size: {dzipInfo.UncompressedSize:N0} bytes");
                sb.AppendLine($"  Compression Header: {string.Join(" ", dzipInfo.CompressionHeader.Select(b => b.ToString("X2")))}");

                dzipInfo.Summary = sb.ToString();

                Logger.Info($"DZIP header detected - Uncompressed size: {dzipInfo.UncompressedSize:N0} bytes");

                return dzipInfo;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error analyzing DZIP header");
                return null;
            }
        }

        /// <summary>
        /// Extract readable strings that might be player names, quest names, etc.
        /// </summary>
        private List<string> FindReadableStrings(byte[] data, int minLength = 4)
        {
            var strings = new List<string>();
            var currentString = new StringBuilder();

            for (int i = 0; i < data.Length; i++)
            {
                var b = data[i];

                if (b >= 32 && b <= 126) // Printable ASCII
                {
                    currentString.Append((char)b);
                }
                else
                {
                    if (currentString.Length >= minLength)
                    {
                        var str = currentString.ToString();
                        if (!string.IsNullOrWhiteSpace(str))
                        {
                            strings.Add($"0x{(i - currentString.Length):X4}: '{str}'");
                        }
                    }
                    currentString.Clear();
                }
            }

            // Don't forget the last string
            if (currentString.Length >= minLength)
            {
                var str = currentString.ToString();
                if (!string.IsNullOrWhiteSpace(str))
                {
                    strings.Add($"0x{(data.Length - currentString.Length):X4}: '{str}'");
                }
            }

            return strings.Take(50).ToList(); // Limit output
        }

        /// <summary>
        /// Find offsets that might contain important data (timestamps, counts, etc.)
        /// </summary>
        private List<string> FindSuspiciousOffsets(byte[] data)
        {
            var suspiciousOffsets = new List<string>();

            // Look for 32-bit integers that might be timestamps (reasonable range)
            for (int i = 0; i <= data.Length - 4; i += 4)
            {
                var value = BitConverter.ToUInt32(data, i);

                // Check if it looks like a Unix timestamp (2020-2030 range)
                if (value >= 1577836800 && value <= 1893456000) // 2020-01-01 to 2030-01-01
                {
                    var date = DateTimeOffset.FromUnixTimeSeconds(value);
                    suspiciousOffsets.Add($"0x{i:X4}: Possible timestamp {value} ({date:yyyy-MM-dd HH:mm:ss})");
                }

                // Check for reasonable level/stat values (1-100)
                if (value >= 1 && value <= 100)
                {
                    suspiciousOffsets.Add($"0x{i:X4}: Possible level/stat {value}");
                }

                // Check for reasonable money amounts (1000-1000000)
                if (value >= 1000 && value <= 1000000)
                {
                    suspiciousOffsets.Add($"0x{i:X4}: Possible money amount {value}");
                }
            }

            return suspiciousOffsets.Take(20).ToList(); // Limit output
        }

        /// <summary>
        /// Look for repeating byte patterns that might indicate arrays or structures
        /// </summary>
        private List<string> FindRepeatingPatterns(byte[] data)
        {
            var patterns = new List<string>();

            // Look for repeating 4-byte patterns (common in structured data)
            var patternCounts = new Dictionary<string, int>();

            for (int i = 0; i <= data.Length - 4; i++)
            {
                var pattern = string.Join("", data.Skip(i).Take(4).Select(b => b.ToString("X2")));
                patternCounts[pattern] = patternCounts.GetValueOrDefault(pattern, 0) + 1;
            }

            foreach (var kvp in patternCounts.Where(p => p.Value >= 3).OrderByDescending(p => p.Value).Take(10))
            {
                patterns.Add($"Pattern {kvp.Key} appears {kvp.Value} times");
            }

            return patterns;
        }

        /// <summary>
        /// Find strings that appear in all save files (likely static structure)
        /// </summary>
        private List<string> FindCommonStrings(List<HexAnalysisResult> analyses)
        {
            if (analyses.Count < 2) return new List<string>();

            var allStrings = analyses.SelectMany(a => a.PossibleStrings).ToList();
            var stringCounts = new Dictionary<string, int>();

            foreach (var str in allStrings)
            {
                stringCounts[str] = stringCounts.GetValueOrDefault(str, 0) + 1;
            }

            return stringCounts
                .Where(kvp => kvp.Value == analyses.Count) // Appears in all files
                .Select(kvp => kvp.Key)
                .ToList();
        }

        /// <summary>
        /// Find byte offsets that change between files
        /// </summary>
        private List<string> FindVariableOffsets(List<HexAnalysisResult> analyses)
        {
            var variableOffsets = new List<string>();

            if (analyses.Count < 2) return variableOffsets;

            var minLength = analyses.Min(a => a.RawData.Length);

            for (int i = 0; i < minLength; i++)
            {
                var bytes = analyses.Select(a => a.RawData[i]).ToList();
                if (bytes.Distinct().Count() > 1) // Different values at this offset
                {
                    var byteStr = string.Join(", ", bytes.Select(b => $"0x{b:X2}"));
                    variableOffsets.Add($"0x{i:X4}: {byteStr}");
                }
            }

            return variableOffsets.Take(50).ToList(); // Limit output
        }

        /// <summary>
        /// Find byte offsets that are the same across all files
        /// </summary>
        private List<string> FindStaticOffsets(List<HexAnalysisResult> analyses)
        {
            var staticOffsets = new List<string>();

            if (analyses.Count < 2) return staticOffsets;

            var minLength = analyses.Min(a => a.RawData.Length);

            for (int i = 0; i < Math.Min(256, minLength); i++) // Check first 256 bytes
            {
                var bytes = analyses.Select(a => a.RawData[i]).ToList();
                if (bytes.Distinct().Count() == 1) // Same value at this offset
                {
                    staticOffsets.Add($"0x{i:X4}: 0x{bytes[0]:X2}");
                }
            }

            return staticOffsets;
        }

        /// <summary>
        /// Generate a comprehensive difference report
        /// </summary>
        private string GenerateDifferenceReport(List<HexAnalysisResult> analyses)
        {
            var sb = new StringBuilder();
            sb.AppendLine("=== Save File Comparison Report ===");
            sb.AppendLine($"Files analyzed: {analyses.Count}");
            sb.AppendLine();

            foreach (var analysis in analyses)
            {
                sb.AppendLine($"{analysis.FileName}: {analysis.FileSize:N0} bytes, modified {analysis.LastModified:yyyy-MM-dd HH:mm:ss}");
            }

            sb.AppendLine();
            sb.AppendLine("This report helps identify:");
            sb.AppendLine("- Static offsets: File structure and headers");
            sb.AppendLine("- Variable offsets: Dynamic game data (progress, stats, etc.)");
            sb.AppendLine("- Common strings: Game identifiers and text");
            sb.AppendLine("- Suspicious values: Timestamps, levels, money amounts");

            return sb.ToString();
        }
    }

    /// <summary>
    /// Result of analyzing a single save file for hex patterns
    /// </summary>
    public class HexAnalysisResult
    {
        public string FilePath { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public DateTime LastModified { get; set; }
        public byte[] RawData { get; set; } = Array.Empty<byte>();
        public string HexDump { get; set; } = string.Empty;
        public List<string> PossibleMagicBytes { get; set; } = new();
        public List<string> PossibleStrings { get; set; } = new();
        public List<string> SuspiciousOffsets { get; set; } = new();
        public List<string> RepeatingPatterns { get; set; } = new();
        public DZipInfo? DZipHeader { get; set; }
        public bool Success { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
    }

    /// <summary>
    /// Information about DZIP compression format found in Witcher 2 save files
    /// </summary>
    public class DZipInfo
    {
        public bool IsDZipFile { get; set; }
        public uint Version { get; set; }
        public uint CompressionType { get; set; }
        public uint DataType { get; set; }
        public uint UncompressedSize { get; set; }
        public byte[] CompressionHeader { get; set; } = new byte[8];
        public string Summary { get; set; } = string.Empty;
    }

    /// <summary>
    /// Result of comparing multiple save files for hex pattern analysis
    /// </summary>
    public class HexComparisonResult
    {
        public List<string> CommonStrings { get; set; } = new();
        public List<string> VariableOffsets { get; set; } = new();
        public List<string> StaticOffsets { get; set; } = new();
        public string DifferenceReport { get; set; } = string.Empty;
        public bool Success { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
    }
}
