using System.ComponentModel;
using WitcherGuiApp.Models;

namespace WitcherGuiApp.ViewModels
{
    public class SaveFileViewModel : INotifyPropertyChanged
    {
        public SaveFile SaveFile { get; }

        public SaveFileViewModel(SaveFile saveFile)
        {
            SaveFile = saveFile;
        }

        public string FileName => SaveFile.FileName;
        public string ModifiedTimeIso => SaveFile.ModifiedTimeIso;
        public int Size => SaveFile.Size;

        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    OnPropertyChanged(nameof(IsSelected));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
