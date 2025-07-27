using System.Globalization;
using System.Threading;
using System.Windows;
using WitcherGuiApp.Utils;

namespace WitcherGuiApp
{
    public partial class App : Application
    {
        public static bool ConfigInitialized { get; private set; }
        public static string ConfigStatusMessage { get; private set; }

        public App()
        {
            InitializeComponent();

            ConfigInitialized = ConfigBootstrapper.EnsureConfigFileExists(out var message);
            ConfigStatusMessage = message;
            
            // Set default culture (will be overridden by language selection)
            Thread.CurrentThread.CurrentUICulture = CultureInfo.CurrentCulture;
            Thread.CurrentThread.CurrentCulture = CultureInfo.CurrentCulture;
        }
    }
}
