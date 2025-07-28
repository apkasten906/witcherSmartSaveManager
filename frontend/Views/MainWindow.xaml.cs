using System.Windows;
using WitcherSmartSaveManager.ViewModels;

namespace WitcherSmartSaveManager.Views
{
    public partial class MainWindow : Window
    {
        private readonly MainViewModel _viewModel = new();

        public MainWindow()
        {
            InitializeComponent();
            DataContext = _viewModel;
        }
        
        // This event handler is needed because we're not using Command binding for the ComboBox
        private void LanguageSelector_Changed(object sender, RoutedEventArgs e)
        {
            // The binding will update the viewmodel, so we don't need to do anything here
            // The two-way binding takes care of updating the SelectedLanguage property
        }

        private async void FetchSaves_Click(object sender, RoutedEventArgs e)
        {
            await _viewModel.LoadSavesAsync();
        }        
    }
}

