using System.Windows;
using WitcherGuiApp.ViewModels;

namespace WitcherGuiApp.Views
{
    public partial class MainWindow : Window
    {
        private readonly MainViewModel _viewModel = new();

        public MainWindow()
        {
            InitializeComponent();
            DataContext = _viewModel;
        }

        private async void FetchSaves_Click(object sender, RoutedEventArgs e)
        {
            await _viewModel.LoadSavesAsync();
        }
    }
}

