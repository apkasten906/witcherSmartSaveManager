using Microsoft.VisualBasic;
using System.ComponentModel;
using System.Windows.Input;
using WitcherGuiApp.Models;

namespace WitcherGuiApp.ViewModels
{
    public class SaveFileViewModel : INotifyPropertyChanged
    {
        public WitcherSaveFile SaveFile { get; }

        public SaveFileViewModel(WitcherSaveFile saveFile)
        {
            SaveFile = saveFile;
            BackupExists = saveFile.BackupExists;
        }

        public string FileName => SaveFile.FileName;
        public string ModifiedTimeIso => SaveFile.ModifiedTimeIso;
        public int Size => SaveFile.Size;
        public bool ScreenshotExists => !string.IsNullOrEmpty(SaveFile?.ScreenshotPath);

        private bool _backupExists = false;
        //get; set; }
        //=> SaveFile.BackupExists;
        public bool BackupExists        
        {
            get => _backupExists;
            set
            {
                _backupExists = value;
                OnPropertyChanged(nameof(BackupExists));                
            }
        }

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
