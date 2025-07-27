using System;
using System.Windows;

namespace WitcherGuiApp.Utils
{
    /// <summary>
    /// Helper class for retrieving localized resources
    /// </summary>
    public static class ResourceHelper
    {
        /// <summary>
        /// Gets a localized string from the resources
        /// </summary>
        /// <param name="key">Resource key</param>
        /// <returns>Localized string</returns>
        public static string GetString(string key)
        {
            try
            {
                return Application.Current.Resources[key] as string;
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
                return string.Format(format, args);
            }
            catch (Exception)
            {
                return $"[{key}]"; // Return a placeholder if the resource is not found
            }
        }
    }
}
