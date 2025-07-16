using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WitcherGuiApp.Views
{
    /// <summary>
    /// Interaktionslogik für OverwiteSaveDialog.xaml
    /// </summary>
    public partial class OverwriteSaveDialog : Window
    {
        public bool OverwriteAllChecked => OverwriteAllCheckBox.IsChecked == true;
        public MessageBoxResult Result { get; private set; } = MessageBoxResult.Cancel;

        public OverwriteSaveDialog(string filename)
        {
            InitializeComponent();
            MessageText.Text = $"File \"{filename}\" already exists. Would you like to overwrite it?";
        }

        private void Yes_Click(object sender, RoutedEventArgs e)
        {
            Result = MessageBoxResult.Yes;
            DialogResult = true;
        }

        private void No_Click(object sender, RoutedEventArgs e)
        {
            Result = MessageBoxResult.No;
            DialogResult = true;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Result = MessageBoxResult.Cancel;
            DialogResult = false;
        }
    }
}
