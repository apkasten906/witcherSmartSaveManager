using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace WitcherCore.Models
{
    public class WitcherSaveFile
    {
        [JsonPropertyName("game")]
        public GameKey Game { get; set; }

        [JsonPropertyName("file_name")]
        public required string FileName { get; set; }

        [JsonPropertyName("modified_time")]
        public double ModifiedTime { get; set; }

        [JsonPropertyName("modified_time_iso")]
        public required string ModifiedTimeIso { get; set; }

        [JsonPropertyName("size")]
        public int Size { get; set; }

        [JsonPropertyName("full_path")]
        public required string FullPath { get; set; }

        [JsonPropertyName("screenshot_path")]
        public string ScreenshotPath { get; set; } = string.Empty;

        [JsonPropertyName("metadata")]
        public Dictionary<string, object> Metadata { get; set; } = new();

        [JsonPropertyName("BackupExists")]
        public bool BackupExists { get; internal set; }
    }
}
