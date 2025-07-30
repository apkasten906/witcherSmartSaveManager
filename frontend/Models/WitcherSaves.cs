using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace WitcherSmartSaveManager.Models
{
    public class WitcherSaveFile
    {
        [JsonPropertyName("game")]
        public GameKey Game { get; set; }

        [JsonPropertyName("file_name")]
        public string FileName { get; set; }

        [JsonPropertyName("modified_time")]
        public double ModifiedTime { get; set; }

        [JsonPropertyName("modified_time_iso")]
        public string ModifiedTimeIso { get; set; }

        [JsonPropertyName("size")]
        public int Size { get; set; }

        [JsonPropertyName("full_path")]
        public string FullPath { get; set; }

        [JsonPropertyName("screenshot_path")]
        public string ScreenshotPath { get; set; }

        [JsonPropertyName("metadata")]
        public Dictionary<string, object> Metadata { get; set; }

        [JsonPropertyName("BackupExists")]
        public bool BackupExists { get; internal set; }
    }
}
