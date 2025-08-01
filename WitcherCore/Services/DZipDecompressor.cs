using System;
using System.IO;
using System.IO.Compression;
using NLog;
using WitcherCore.Models;

namespace WitcherCore.Services
{
    /// <summary>
    /// DZIP decompression service for Witcher 2 save files
    /// Phase 1.1: Implements decompression to access uncompressed save data structure
    /// </summary>
    public class DZipDecompressor
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Decompress a DZIP-formatted save file
        /// Based on Phase 1 analysis: DZIP is a variant of standard compression
        /// </summary>
        public DecompressionResult DecompressSaveFile(string filePath)
        {
            var result = new DecompressionResult
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

                var compressedData = File.ReadAllBytes(filePath);

                // Verify DZIP header
                if (!VerifyDZipHeader(compressedData, result))
                {
                    return result;
                }

                // Extract compression parameters from header
                var header = ParseDZipHeader(compressedData);
                result.Header = header;

                // Attempt decompression based on discovered format
                result.UncompressedData = DecompressDZipData(compressedData, header);

                if (result.UncompressedData != null)
                {
                    result.Success = true;
                    result.UncompressedSize = result.UncompressedData.Length;
                    result.CompressionRatio = (double)compressedData.Length / result.UncompressedData.Length;

                    Logger.Info($"Successfully decompressed {filePath}: {compressedData.Length} -> {result.UncompressedSize} bytes");
                }
                else
                {
                    result.ErrorMessage = "All decompression strategies failed - unknown DZIP compression format";
                    Logger.Warn($"Failed to decompress {filePath} - all strategies exhausted");
                }

                return result;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"Error decompressing DZIP file: {filePath}");
                result.ErrorMessage = $"Decompression failed: {ex.Message}";
                return result;
            }
        }

        /// <summary>
        /// Verify DZIP magic bytes and basic header structure
        /// </summary>
        private bool VerifyDZipHeader(byte[] data, DecompressionResult result)
        {
            if (data.Length < 24)
            {
                result.ErrorMessage = "File too small to contain DZIP header";
                return false;
            }

            // Check magic bytes "DZIP"
            if (!(data[0] == 0x44 && data[1] == 0x5A && data[2] == 0x49 && data[3] == 0x50))
            {
                result.ErrorMessage = "Not a DZIP file - missing magic bytes";
                return false;
            }

            return true;
        }

        /// <summary>
        /// Parse DZIP header structure discovered in Phase 1
        /// </summary>
        private DZipHeader ParseDZipHeader(byte[] data)
        {
            return new DZipHeader
            {
                MagicBytes = new byte[] { data[0], data[1], data[2], data[3] },
                Version = BitConverter.ToUInt32(data, 4),
                CompressionType = BitConverter.ToUInt32(data, 8),
                DataType = BitConverter.ToUInt32(data, 12),
                UncompressedSize = BitConverter.ToUInt32(data, 16),
                Reserved = BitConverter.ToUInt32(data, 20),
                CompressionHeader = new byte[8]
            };
        }

        /// <summary>
        /// Attempt to decompress DZIP data using multiple strategies
        /// </summary>
        private byte[]? DecompressDZipData(byte[] compressedData, DZipHeader header)
        {
            // Extract payload data (everything after 24-byte header)
            var compressedPayload = new byte[compressedData.Length - 24];
            Array.Copy(compressedData, 24, compressedPayload, 0, compressedPayload.Length);

            // Based on analysis: DZIP appears to be uncompressed wrapper format!
            // Compression ratio is consistently ~100% (1:1)
            if (Math.Abs(header.UncompressedSize - compressedPayload.Length) <= 100) // Allow small variance
            {
                Logger.Info("DZIP appears to be uncompressed wrapper format - returning payload directly");
                return compressedPayload;
            }

            // Fallback: Try standard compression methods if ratio suggests actual compression
            Logger.Debug($"Expected size {header.UncompressedSize}, payload size {compressedPayload.Length} - trying compression methods");

            // Strategy 1: Try deflate decompression
            try
            {
                return DecompressDeflate(compressedPayload);
            }
            catch (Exception ex)
            {
                Logger.Debug($"Deflate decompression failed: {ex.Message}");
            }

            // Strategy 2: Try GZip decompression
            try
            {
                return DecompressGZip(compressedPayload);
            }
            catch (Exception ex)
            {
                Logger.Debug($"GZip decompression failed: {ex.Message}");
            }

            Logger.Warn("All decompression strategies failed");
            return null;
        }

        /// <summary>
        /// Decompress using standard Deflate algorithm
        /// </summary>
        private byte[] DecompressDeflate(byte[] compressedData)
        {
            using var input = new MemoryStream(compressedData);
            using var deflate = new DeflateStream(input, CompressionMode.Decompress);
            using var output = new MemoryStream();

            deflate.CopyTo(output);
            return output.ToArray();
        }

        /// <summary>
        /// Decompress using GZip algorithm  
        /// </summary>
        private byte[] DecompressGZip(byte[] compressedData)
        {
            using var input = new MemoryStream(compressedData);
            using var gzip = new GZipStream(input, CompressionMode.Decompress);
            using var output = new MemoryStream();

            gzip.CopyTo(output);
            return output.ToArray();
        }
    }
}
