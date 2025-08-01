using System;

namespace WitcherCore.Models
{
    /// <summary>
    /// Header information from a Witcher 2 save file
    /// </summary>
    public class SaveFileHeader
    {
        public string Version { get; set; } = string.Empty;
        public string PlayerName { get; set; } = string.Empty;
        public int Level { get; set; }
        public uint PlaytimeMinutes { get; set; }
        public DateTime SaveTime { get; set; }
        public string Location { get; set; } = string.Empty;
        public string Chapter { get; set; } = string.Empty;
        public int Money { get; set; }
    }
}
