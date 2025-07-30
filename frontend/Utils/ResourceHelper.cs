using System;
using System.Globalization;
using System.Resources;
using System.Windows;

namespace WitcherSmartSaveManager.Utils
{
    /// <summary>
    /// Helper class for retrieving localized resources
    /// </summary>
    public static class ResourceHelper
    {
        private static System.Resources.ResourceManager _resourceManager;

        static ResourceHelper()
        {
            // Initialize the resource manager for the Strings.resx files
            _resourceManager = new System.Resources.ResourceManager("WitcherSmartSaveManager.Resources.Strings", typeof(ResourceHelper).Assembly);
        }

        /// <summary>
        /// Gets a localized string from the resources
        /// </summary>
        /// <param name="key">Resource key</param>
        /// <returns>Localized string</returns>
        public static string GetString(string key)
        {
            try
            {
                var result = _resourceManager.GetString(key, CultureInfo.CurrentUICulture);
                return result ?? $"[{key}]"; // Return a placeholder if the resource is not found
            }
            catch (Exception)
            {
                return $"[{key}]"; // Return a placeholder if the resource is not found
            }
        }

        /// <summary>
        /// Gets a formatted localized string from the resources
        /// </summary>
        /// <param name="key">Resource key</param>
        /// <param name="args">Format arguments</param>
        /// <returns>Formatted localized string</returns>
        public static string GetFormattedString(string key, params object[] args)
        {
            try
            {
                string format = GetString(key);
                if (format.StartsWith("[") && format.EndsWith("]"))
                {
                    return format; // Return the placeholder if the key wasn't found
                }
                return string.Format(format, args);
            }
            catch (Exception)
            {
                return $"[{key}]"; // Return a placeholder if the resource is not found
            }
        }
    }
}
