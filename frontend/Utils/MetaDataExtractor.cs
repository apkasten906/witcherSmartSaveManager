using System;
using System.Collections.Generic;
using WitcherSmartSaveManager.Models;

namespace WitcherSmartSaveManager.Utils
{
    public static class MetadataExtractor
    {
        public static void PopulateMetadata(WitcherSaveFile save)
        {
            // Placeholder logic – replace with actual parsing later
            save.Metadata["source"] = "mock";
            save.Metadata["quest"] = "The Little Kayran That Could";
            save.Metadata["timestamp_iso"] = save.ModifiedTimeIso;
        }

        public static Dictionary<string, object> GetMetadata(string file)
        {
            var metadata = new Dictionary<string, object>();
            // Placeholder logic – replace with actual parsing later
            metadata["source"] = "mock";
            metadata["quest"] = "The Little Kayran That Could";

            return metadata;
        }
    }
}
