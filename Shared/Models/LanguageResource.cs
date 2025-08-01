using System;

#nullable enable

namespace WitcherCore.Models
{
    /// <summary>
    /// Represents a localized resource entry for database storage.
    /// Part of the optional database enhancement layer for advanced localization features.
    /// </summary>
    public class LanguageResource
    {
        /// <summary>
        /// The unique identifier for the resource (e.g., "Status_Ready", "TotalSaveFiles")
        /// </summary>
        public required string Key { get; set; }

        /// <summary>
        /// The localized text value for this resource
        /// </summary>
        public required string Value { get; set; }

        /// <summary>
        /// The language/culture code (e.g., "en", "de", "en-US")
        /// </summary>
        public string Language { get; set; } = "en";

        /// <summary>
        /// Optional category for grouping resources (e.g., "UI", "Errors", "Status")
        /// </summary>
        public string? Category { get; set; }

        /// <summary>
        /// When this resource was last updated
        /// </summary>
        public DateTime LastModified { get; set; } = DateTime.UtcNow;
    }
}
