using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Microsoft.WindowsAPICodePack.Dialogs;
using WitcherGuiApp.Services;
using WitcherGuiApp.Utils;
using WitcherGuiApp.Views;

namespace WitcherGuiApp.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly WitcherSaveFileService _saveFileService;
        private readonly string _gameKey;

        
        public ObservableCollection<SaveFileViewModel> Saves { get; set; } = new();

        private string _statusMessage;
        public string StatusMessage
        {
            get => _statusMessage;
            set
            {
                if (_statusMessage != value)
                {
                    _statusMessage = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _backupFilePath;

        public string BackupFilePath
        {
            get => _backupFilePath;

            set
            {
                if (_backupFilePath != value)
                {
                    _backupFilePath = value;
                    
                    OnPropertyChanged();
                }                       
            }
        }

        /// <summary>overwrite all files without prompting when backing up</summary>
        private bool _overwriteAll;

        public bool AreAllSelected
        {
            get => Saves.Count > 0 && Saves.All(s => s.IsSelected);
            set
            {
                foreach (var save in Saves)
                {
                    save.IsSelected = value;
                }

                OnPropertyChanged(); // force header update
            }
        }


        public MainViewModel()
        {
            StatusMessage = App.ConfigStatusMessage;

            if (!App.ConfigInitialized)
            {
                // Config failed, don't initialize service
                return;
            }

            _gameKey = "Witcher2";
            _saveFileService = new WitcherSaveFileService(_gameKey);

            _backupFilePath = SavePathResolver.GetDefaultBackupPath(_gameKey);

            DeleteSelectedCommand = new RelayCommand(_ => DeleteSelectedSaves(), _ => Saves.Any(s => s.IsSelected));
            BackupSelectedCommand = new RelayCommand(_ => BackupSelectedSaves(), _ => Saves.Any(s => s.IsSelected));
            BackupLocationPickerCommand = new RelayCommand(_ => SelectBackupFolder());
        }

        public ICommand DeleteSelectedCommand { get; }

        // Replace the problematic code block with the following:
        public ICommand BackupLocationPickerCommand { get; }
        public ICommand BackupSelectedCommand { get; } 

        private void Save_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(SaveFileViewModel.IsSelected))
            {
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    OnPropertyChanged(nameof(AreAllSelected));
                });

                CommandManager.InvalidateRequerySuggested();
            }
        }

        public async Task LoadSavesAsync()
        {
            if (_saveFileService == null)
            {
                StatusMessage = "Cannot load saves: configuration not initialized.";
                return;
            }

            StatusMessage = "Loading save files...";
            Saves.Clear();

            try
            {
                await Task.Run(() =>
                {
                    var saveFiles = _saveFileService.GetSaveFiles();

                    foreach (var save in saveFiles)
                    {
                        App.Current.Dispatcher.Invoke(() =>
                        {
                            var vm = new SaveFileViewModel(save);
                            vm.PropertyChanged += Save_PropertyChanged;
                            Saves.Add(vm);
                        });
                    }

                    StatusMessage = saveFiles.Count == 0
                        ? "No save files found."
                        : $"{saveFiles.Count} save(s) found.";
                });
            }
            catch (System.Exception ex)
            {
                StatusMessage = $"Error: {ex.Message}";
            }
        }

        public void SelectBackupFolder()
        {
            var dlg = new CommonOpenFileDialog
            {
                IsFolderPicker = true,
                Title = "Select Backup Folder",
                EnsurePathExists = true
            };

            if (dlg.ShowDialog() == CommonFileDialogResult.Ok)
            {
                BackupFilePath = dlg.FileName;                
            }        
        }

        private void BackupSelectedSaves()
        {
            var selected = Saves.Where(s => s.IsSelected).ToList();

            try { 

                foreach (var save in selected)
                {
                    // default backup path is the save folder with "_backup" suffix
                    if (string.IsNullOrWhiteSpace(_backupFilePath))
                    {
                        _backupFilePath = SavePathResolver.GetSavePath(_gameKey) + "\\_backup";
                    }

                    if (!Directory.Exists(_backupFilePath))
                    {
                        Directory.CreateDirectory(_backupFilePath);
                    }

                    var fileName = Path.GetFileName(save.FileName);
                    var destPath = Path.Combine(_backupFilePath, fileName);

                    // Check if file already exists and prompt for overwrite
                    if (File.Exists(destPath) && !_overwriteAll)
                    {
                        var dialog = new OverwriteSaveDialog(fileName)
                        {
                            Owner = System.Windows.Application.Current?.MainWindow
                        };

                        var dialogResult = dialog.ShowDialog();

                        if (dialog.Result == MessageBoxResult.Yes)
                        {
                            _overwriteAll = dialog.OverwriteAllChecked;                        
                        }
                        else if (dialog.Result == MessageBoxResult.No)
                        {
                            continue;
                        }
                        else if (dialog.Result == MessageBoxResult.Cancel)
                        {
                            break;
                        }                    
                    }

                    _saveFileService.BackupSaveFile(save.SaveFile.FullPath, _gameKey, _backupFilePath, _overwriteAll);
                }

            }
            catch (Exception ex)
            {
                StatusMessage = $"Error backing up saves: {ex.Message}";
                return;
            }

            StatusMessage = $"{selected.Count} save(s) backed up.";
        }

        private void DeleteSelectedSaves()
        {
            var selected = Saves.Where(s => s.IsSelected).ToList();

            foreach (var save in selected)
            {
                bool deleted = _saveFileService.DeleteSaveFile(save.SaveFile.FullPath);
                if (deleted)
                {
                    Saves.Remove(save);
                }
            }

            StatusMessage = $"{selected.Count} save(s) deleted.";
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            Console.WriteLine($"PropertyChanged: {name}");
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
