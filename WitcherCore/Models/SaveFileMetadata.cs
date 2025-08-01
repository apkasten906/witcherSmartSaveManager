using System;
using System.Collections.Generic;

namespace WitcherCore.Models
{
    public class SaveFileMetadata
    {
        public string FileName { get; set; }
        public DateTime LastModified { get; set; }
        public long FileSize { get; set; }
        public string GameKey { get; set; }
        public Dictionary<string, object> AdditionalMetadata { get; set; } = new Dictionary<string, object>();

        public SaveFileMetadata(string fileName, DateTime lastModified, long fileSize, string gameKey)
        {
            FileName = fileName;
            LastModified = lastModified;
            FileSize = fileSize;
            GameKey = gameKey;
        }

        public void AddMetadata(string key, object value)
        {
            if (!AdditionalMetadata.ContainsKey(key))
            {
                AdditionalMetadata[key] = value;
            }
        }

        public object? GetMetadata(string key)
        {
            return AdditionalMetadata.TryGetValue(key, out var value) ? value : null;
        }
    }
}