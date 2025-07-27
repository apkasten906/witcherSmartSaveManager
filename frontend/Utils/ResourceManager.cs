using System.Globalization;
using System.Windows;

namespace WitcherGuiApp.Utils
{
    /// <summary>
    /// Utility class for managing resources and language switching
    /// </summary>
    public static class ResourceManager
    {
        /// <summary>
        /// Force a refresh of all WPF resources to reflect language changes
        /// </summary>
        public static void UpdateResources()
        {
            // Update resource dictionaries for the current culture
            ResourceDictionaryHelper.UpdateResourcesForCulture(CultureInfo.CurrentUICulture);
            
            // Get all resource dictionaries and refresh them
            foreach (ResourceDictionary dict in Application.Current.Resources.MergedDictionaries)
            {
                // If this is a resource dictionary with a source, refresh it
                if (dict.Source != null)
                {
                    var source = dict.Source;
                    dict.Source = null;
                    dict.Source = source;
                }
            }

            // Force update of all windows
            foreach (Window window in Application.Current.Windows)
            {
                // Update the window layout
                window.UpdateLayout();
            }
        }
    }
}
