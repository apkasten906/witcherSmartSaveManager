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
using WitcherGuiApp.Models;
using WitcherGuiApp.Services;
using WitcherGuiApp.Utils;
using WitcherGuiApp.Views;

namespace WitcherGuiApp.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly WitcherSaveFileService _saveFileService;
        private readonly GameKey _gameKey;

        
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

            _gameKey = GameKey.Witcher2;
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

                RefreshBackupStates();
            }
        }

        private void BackupSelectedSaves()
        {                        
            try 
            {
                // default backup path is the save folder with "_backup" suffix
                if (string.IsNullOrWhiteSpace(_backupFilePath))
                {
                    _backupFilePath = SavePathResolver.GetDefaultBackupPath(_gameKey);
                }

                if (!Directory.Exists(_backupFilePath))
                {
                    // add prompt to confirm creation of folder is allowed
                    var confirmFolderCreationDialog = MessageBox.Show(Window.GetWindow(Application.Current.MainWindow),
                        "The backup folder does not exist. Do you want to create it?",
                        "Create Backup Folder", MessageBoxButton.YesNo, MessageBoxImage.Question);

                    if (confirmFolderCreationDialog == MessageBoxResult.No)
                    {
                        StatusMessage = "Backup cancelled. Folder not created.";
                        return; // user chose not to create folder
                    }

                    Directory.CreateDirectory(_backupFilePath);
                }
                
                var selected = Saves.Where(s => s.IsSelected).ToList();                

                foreach (var save in selected)
                {                        

                    var fileName = Path.GetFileName(save.FileName);
                    var destPath = Path.Combine(_backupFilePath, fileName);
                    var overwrite = _overwriteAll;

                    // Check if file already exists and prompt for overwrite
                    if (File.Exists(destPath) && !_overwriteAll)
                    {
                        var dialog = new OverwriteSaveDialog(fileName)
                        {
                            Owner = Application.Current?.MainWindow
                        };

                        var dialogResult = dialog.ShowDialog();

                        if (dialog.Result == MessageBoxResult.Yes)
                        {
                            _overwriteAll = dialog.OverwriteAllChecked;
                            overwrite = true; // user chose to overwrite this file
                        }
                        else if (dialog.Result == MessageBoxResult.No)
                        {
                            _overwriteAll = dialog.OverwriteAllChecked;
                            overwrite = false; // user chose not to overwrite this file, but will overwrite others if checked
                            continue;
                        }
                        else if (dialog.Result == MessageBoxResult.Cancel)
                        {
                            break;
                        }
                    }

                    var backupResult = _saveFileService.BackupSaveFile(save.SaveFile, overwrite);

                    if (backupResult)
                    {
                        //save.SaveFile.BackupExists = true; // Update the backup status in the view model
                        save.BackupExists = true; // Update the backup status in the view model
                        StatusMessage = $"Backup Successful";
                    }
                }

                StatusMessage = $"{selected.Count} save(s) backed up.";

            }
            catch (Exception ex)
            {
                StatusMessage = $"Error backing up saves: {ex.Message}";
                return;
            }            
        }

        private void RefreshBackupStates()
        {
            foreach (var save in Saves)
            {
                var backupPath = Path.Combine(BackupFilePath, Path.GetFileName(save.FileName));
                save.BackupExists = File.Exists(backupPath);
            }
        }

        private void DeleteSelectedSaves()
        {
            try
            {

                var selected = Saves.Where(s => s.IsSelected).ToList();

                foreach (var save in selected)
                {
                    var deleteResult = _saveFileService.DeleteSaveFile(save.SaveFile);

                    if (deleteResult)
                    {
                        Saves.Remove(save);
                        StatusMessage = $"Save deleted.";
                    }
                }

                StatusMessage = $"{selected.Count} save(s) deleted.";

            }
            catch(Exception ex)
            {
                StatusMessage = $"Error backing up saves: {ex.Message}";
                return;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {            
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
