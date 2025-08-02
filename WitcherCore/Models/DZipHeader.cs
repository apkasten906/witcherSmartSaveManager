namespace WitcherCore.Models
{
    /// <summary>
    /// DZIP header structure discovered in Phase 1 analysis
    /// Contains compression metadata for Witcher 2 save files
    /// </summary>
    public class DZipHeader
    {
        public byte[] MagicBytes { get; set; } = new byte[4];
        public uint Version { get; set; }
        public uint CompressionType { get; set; }
        public uint DataType { get; set; }
        public uint UncompressedSize { get; set; }
        public uint Reserved { get; set; }
        public byte[] CompressionHeader { get; set; } = new byte[8];

        public bool IsValid =>
            MagicBytes.Length == 4 &&
            MagicBytes[0] == 0x44 &&
            MagicBytes[1] == 0x5A &&
            MagicBytes[2] == 0x49 &&
            MagicBytes[3] == 0x50;
    }
}
