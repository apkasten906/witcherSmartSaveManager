namespace WitcherCore.Models
{
    /// <summary>
    /// Result of DZIP decompression operation
    /// </summary>
    public class DecompressionResult
    {
        public string FilePath { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public DZipHeader? Header { get; set; }
        public byte[]? UncompressedData { get; set; }
        public int UncompressedSize { get; set; }
        public double CompressionRatio { get; set; }
        public bool Success { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
    }
}
