using System;
using System.Globalization;
using System.IO;
using System.Windows;

namespace WitcherSmartSaveManager.Utils
{
    /// <summary>
    /// Utility class to help with dynamically loading resource dictionaries
    /// based on the current culture.
    /// </summary>
    public static class ResourceDictionaryHelper
    {
        /// <summary>
        /// Loads a resource dictionary for the specified culture.
        /// </summary>
        /// <param name="baseName">Base name of the resource dictionary.</param>
        /// <param name="culture">Culture to load.</param>
        /// <returns>A ResourceDictionary for the specified culture, or null if it doesn't exist.</returns>
        public static ResourceDictionary LoadResourceDictionary(string baseName, CultureInfo culture)
        {
            try
            {
                // Try to load the resource dictionary for the specific culture
                string cultureSuffix = culture.Name;
                string resourcePath = $"/Resources/{baseName}.{cultureSuffix}.xaml";

                // First check if culture-specific resource exists
                var uri = new Uri($"pack://application:,,,{resourcePath}", UriKind.Absolute);

                try
                {
                    return new ResourceDictionary { Source = uri };
                }
                catch
                {
                    // If culture-specific resource doesn't exist, fall back to default
                    resourcePath = $"/Resources/{baseName}.xaml";
                    uri = new Uri($"pack://application:,,,{resourcePath}", UriKind.Absolute);
                    return new ResourceDictionary { Source = uri };
                }
            }
            catch (Exception ex)
            {
                // Log any exceptions
                System.Diagnostics.Debug.WriteLine($"Error loading resource dictionary: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Updates the application's resource dictionaries based on the current culture.
        /// </summary>
        /// <param name="culture">The culture to use.</param>
        public static void UpdateResourcesForCulture(CultureInfo culture)
        {
            // Remove any existing localized resource dictionaries
            for (int i = Application.Current.Resources.MergedDictionaries.Count - 1; i >= 0; i--)
            {
                var dict = Application.Current.Resources.MergedDictionaries[i];
                if (dict.Source != null && dict.Source.ToString().Contains("/Resources/StringResources"))
                {
                    Application.Current.Resources.MergedDictionaries.RemoveAt(i);
                }
            }

            // Try to load the culture-specific resource dictionary
            var resourceDict = LoadResourceDictionary("StringResources", culture);
            if (resourceDict != null)
            {
                Application.Current.Resources.MergedDictionaries.Add(resourceDict);
            }
        }
    }
}
