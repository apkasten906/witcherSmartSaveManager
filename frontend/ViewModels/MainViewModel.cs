using System.Globalization;
using System.Threading;
using WitcherSmartSaveManager.Models;
using WitcherSmartSaveManager.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using Microsoft.WindowsAPICodePack.Dialogs;
using WitcherSmartSaveManager.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;

namespace WitcherSmartSaveManager.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private string _selectedLanguage = "en";
        private string _originalConfigMessage; // Store the original config message

        public string SelectedLanguage
        {
            get => _selectedLanguage;
            set
            {
                if (_selectedLanguage != value)
                {
                    _selectedLanguage = value;
                    SetLanguage(_selectedLanguage);
                    OnPropertyChanged();
                    // Raise property changed for all localized properties if needed
                }
            }
        }

        public ICommand ChangeLanguageCommand { get; }

        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly WitcherSaveFileService _saveFileService;
        private readonly GameKey _gameKey;
        private string _statusMessage;
        private string _backupFilePath;
        public ObservableCollection<SaveFileViewModel> Saves { get; set; } = new();

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

        public bool AreAllSelected
        {
            get => Saves.Count > 0 && Saves.All(s => s.IsSelected);
            set
            {
                foreach (var save in Saves)
                {
                    save.IsSelected = value;
                }
                OnPropertyChanged();
            }
        }

        private int _totalSaveFiles;
        public int TotalSaveFiles
        {
            get => _totalSaveFiles;
            set
            {
                if (_totalSaveFiles != value)
                {
                    _totalSaveFiles = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(TotalSaveFilesDisplay));
                }
            }
        }

        private int _backedUpFiles;
        public int BackedUpFiles
        {
            get => _backedUpFiles;
            set
            {
                if (_backedUpFiles != value)
                {
                    _backedUpFiles = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(BackedUpFilesDisplay));
                }
            }
        }

        public string TotalSaveFilesDisplay => Utils.ResourceHelper.GetFormattedString("TotalSaveFiles", TotalSaveFiles);
        public string BackedUpFilesDisplay => Utils.ResourceHelper.GetFormattedString("TotalBackedUpFiles", BackedUpFiles);

        public ICommand CustomSaveFolderPickerCommand { get; }
        public ICommand DeleteSelectedCommand { get; }
        public ICommand BackupSelectedCommand { get; }
        public ICommand BackupLocationPickerCommand { get; }

        public string CustomSaveFolderPath { get; set; }

        public MainViewModel()
        {
            // Initialize counters
            TotalSaveFiles = 0;
            BackedUpFiles = 0;

            ChangeLanguageCommand = new RelayCommand(lang => SelectedLanguage = lang?.ToString());

            Logger.Info("MainViewModel initialized.");

            // Store the original config message for later localization
            _originalConfigMessage = App.ConfigStatusMessage;

            // Set initial status message
            UpdateConfigStatusMessage();

            if (!App.ConfigInitialized)
            {
                return;
            }
            _gameKey = GameKey.Witcher2;
            _saveFileService = new WitcherSaveFileService(_gameKey);
            _backupFilePath = SavePathResolver.GetDefaultBackupPath(_gameKey);
            CustomSaveFolderPath = SavePathResolver.GetSavePath(_gameKey);
            DeleteSelectedCommand = new RelayCommand(_ => DeleteSelectedSaves(), _ => Saves.Any(s => s.IsSelected));
            BackupSelectedCommand = new RelayCommand(_ => BackupSelectedSaves(), _ => Saves.Any(s => s.IsSelected));
            BackupLocationPickerCommand = new RelayCommand(_ => SelectBackupFolder());
            CustomSaveFolderPickerCommand = new RelayCommand(_ => SelectCustomSaveFolder());
            SetLanguage(_selectedLanguage);

            // Initialize backup counter
            UpdateBackupFileCounter();
        }

        private void SetLanguage(string lang)
        {
            var culture = new CultureInfo(lang);
            Thread.CurrentThread.CurrentUICulture = culture;
            Thread.CurrentThread.CurrentCulture = culture;

            // Notify that all bindable properties have changed to force UI refresh
            Application.Current.Dispatcher.Invoke(() =>
            {
                try
                {
                    // Load the appropriate resource dictionary for the selected language
                    Utils.ResourceDictionaryHelper.UpdateResourcesForCulture(culture);

                    // Update status message with localized text if it's a standard message
                    UpdateLocalizedStatusMessage();

                    // Explicitly update config status message when language changes
                    UpdateConfigStatusMessage();

                    // Force property changed notifications for all localized properties
                    OnPropertyChanged(nameof(StatusMessage));
                    OnPropertyChanged(nameof(TotalSaveFilesDisplay));
                    OnPropertyChanged(nameof(BackedUpFilesDisplay));

                    // Refresh UI
                    foreach (Window window in Application.Current.Windows)
                    {
                        window.UpdateLayout();
                    }

                    // Log success
                    Logger.Info($"Language changed to: {lang}");
                }
                catch (Exception ex)
                {
                    // Log any errors
                    Logger.Error(ex, $"Error changing language to {lang}");
                }
            });
        }

        /// <summary>
        /// Updates the status message if it needs to be localized
        /// </summary>
        private void UpdateLocalizedStatusMessage()
        {
            try
            {
                // If the status message is empty, set it to the default ready message
                if (string.IsNullOrEmpty(_statusMessage))
                {
                    StatusMessage = Utils.ResourceHelper.GetString("Status_Ready");
                    return;
                }

                // Try to update known status message patterns
                UpdateStatusMessageIfMatches();

                // Note: Config status message is handled separately by UpdateConfigStatusMessage
                // to preserve the original config message format across language changes
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error updating localized status message");
            }
        }

        /// <summary>
        /// Updates the status message if it matches one of our known patterns
        /// </summary>
        private void UpdateStatusMessageIfMatches()
        {
            // Check for standard patterns and update if matched
            if (_statusMessage.StartsWith("Loaded ") && _statusMessage.EndsWith(" save(s)."))
            {
                // Extract the number of saves
                var saveCount = _statusMessage.Replace("Loaded ", "").Replace(" save(s).", "");
                StatusMessage = Utils.ResourceHelper.GetFormattedString("Status_SavesLoaded", saveCount);
            }
            else if (_statusMessage.EndsWith(" save(s) backed up."))
            {
                var saveCount = _statusMessage.Replace(" save(s) backed up.", "");
                StatusMessage = Utils.ResourceHelper.GetFormattedString("Status_BackupComplete", saveCount);
            }
            else if (_statusMessage.EndsWith(" save(s) deleted."))
            {
                var saveCount = _statusMessage.Replace(" save(s) deleted.", "");
                StatusMessage = Utils.ResourceHelper.GetFormattedString("Status_DeleteComplete", saveCount);
            }
            else if (_statusMessage == "Save deleted.")
            {
                StatusMessage = Utils.ResourceHelper.GetString("Status_SaveDeleted");
            }
            else if (_statusMessage == "Backup folder set.")
            {
                StatusMessage = Utils.ResourceHelper.GetString("Status_BackupFolderSet");
            }
            else if (_statusMessage == "Custom save folder set.")
            {
                StatusMessage = Utils.ResourceHelper.GetString("Status_CustomSaveFolderSet");
            }
            else if (_statusMessage == "Selected folder does not contain Witcher 2 save files.")
            {
                StatusMessage = Utils.ResourceHelper.GetString("Status_NoSaveFiles");
            }
            else if (_statusMessage.StartsWith("Error loading saves: "))
            {
                var errorMsg = _statusMessage.Replace("Error loading saves: ", "");
                StatusMessage = Utils.ResourceHelper.GetFormattedString("Error_LoadingSaves", errorMsg);
            }
            else if (_statusMessage.StartsWith("Error backing up saves: "))
            {
                var errorMsg = _statusMessage.Replace("Error backing up saves: ", "");
                StatusMessage = Utils.ResourceHelper.GetFormattedString("Error_BackupSaves", errorMsg);
            }
            else if (_statusMessage.StartsWith("Error deleting saves: "))
            {
                var errorMsg = _statusMessage.Replace("Error deleting saves: ", "");
                StatusMessage = Utils.ResourceHelper.GetFormattedString("Error_DeletingSaves", errorMsg);
            }
        }

        /// <summary>
        /// Updates the config status message based on the original stored message
        /// </summary>
        private void UpdateConfigStatusMessage()
        {
            if (_originalConfigMessage == null)
                return;

            if (_originalConfigMessage.StartsWith("Config file found at "))
            {
                string path = _originalConfigMessage.Replace("Config file found at ", "").TrimEnd('.');
                StatusMessage = Utils.ResourceHelper.GetFormattedString("Status_ConfigFound", path);
            }
            else if (_originalConfigMessage.StartsWith("Created default config file at: "))
            {
                string path = _originalConfigMessage.Replace("Created default config file at: ", "");
                StatusMessage = Utils.ResourceHelper.GetFormattedString("Status_ConfigFound", path);
            }
            else if (_originalConfigMessage.StartsWith("Failed to create config file"))
            {
                StatusMessage = Utils.ResourceHelper.GetString("Status_ConfigNotFound");
            }
        }

        /// <summary>
        /// Updates the backup file counter by counting actual files in the backup folder
        /// </summary>
        private void UpdateBackupFileCounter()
        {
            if (_saveFileService != null)
            {
                var count = _saveFileService.GetBackupFileCount();
                BackedUpFiles = count;
            }
            else
            {
                Logger.Warn("UpdateBackupFileCounter: _saveFileService is null");
            }
        }

        public async Task LoadSavesAsync()
        {
            try
            {
                // Simulate async load (replace with actual async service if available)
                await Task.Run(() =>
                {
                    var saves = _saveFileService.GetSaveFiles();
                    App.Current.Dispatcher.Invoke(() =>
                    {
                        Saves.Clear();
                        foreach (var save in saves)
                        {
                            Saves.Add(new SaveFileViewModel(save));
                        }

                        // Update counters
                        TotalSaveFiles = Saves.Count;
                        BackedUpFiles = _saveFileService.GetBackupFileCount();
                    });
                });

                // Check for orphaned screenshots after loading saves
                var orphanedScreenshots = _saveFileService.GetOrphanedScreenshots();
                if (orphanedScreenshots.Count > 0)
                {
                    HandleOrphanedScreenshots(orphanedScreenshots);
                }

                // Use localized status message format
                StatusMessage = Utils.ResourceHelper.GetFormattedString("Status_SavesLoaded", Saves.Count);
            }
            catch (Exception ex)
            {
                StatusMessage = Utils.ResourceHelper.GetFormattedString("Error_LoadingSaves", ex.Message);
            }
        }

        private void HandleOrphanedScreenshots(List<string> orphanedScreenshots)
        {
            try
            {
                var witchyExplanation = Utils.ResourceHelper.GetFormattedString("OrphanedScreenshots_Message", orphanedScreenshots.Count);
                var title = Utils.ResourceHelper.GetString("OrphanedScreenshots_Title");

                var result = MessageBox.Show(witchyExplanation, title, MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    var (deletedCount, lockedFiles) = _saveFileService.CleanupOrphanedScreenshotsWithDetails(orphanedScreenshots);

                    if (lockedFiles.Any())
                    {
                        var fileList = string.Join("\n", lockedFiles.Select(f => $"• {Path.GetFileName(f.file)}: {f.reason}"));
                        var lockedMessage = Utils.ResourceHelper.GetFormattedString("OrphanedScreenshots_PartialCleanup_Message",
                            deletedCount, orphanedScreenshots.Count, lockedFiles.Count, fileList);
                        var lockedTitle = Utils.ResourceHelper.GetString("OrphanedScreenshots_PartialCleanup_Title");

                        MessageBox.Show(lockedMessage, lockedTitle, MessageBoxButton.OK, MessageBoxImage.Information);
                        StatusMessage = Utils.ResourceHelper.GetFormattedString("Status_OrphanedPartialCleanup",
                            deletedCount, orphanedScreenshots.Count, lockedFiles.Count);
                    }
                    else
                    {
                        StatusMessage = Utils.ResourceHelper.GetFormattedString("Status_OrphanedFullCleanup", deletedCount);
                    }
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error handling orphaned screenshots: {ex.Message}";
            }
        }

        public void BackupSelectedSaves()
        {
            try
            {
                var selected = Saves.Where(s => s.IsSelected).ToList();
                foreach (var save in selected)
                {
                    var backupResult = _saveFileService.BackupSaveFile(save.SaveFile, true);
                    if (backupResult)
                    {
                        save.BackupExists = true;
                    }
                }

                // Update backup counter
                BackedUpFiles = _saveFileService.GetBackupFileCount();

                // Use localized status message format
                StatusMessage = Utils.ResourceHelper.GetFormattedString("Status_BackupComplete", selected.Count);
            }
            catch (Exception ex)
            {
                StatusMessage = Utils.ResourceHelper.GetFormattedString("Error_BackupSaves", ex.Message);
            }
        }

        private void UpdateUserPathsJson(string folderPath)
        {
            var jsonPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "userpaths.json");
            dynamic jsonObj;
            if (File.Exists(jsonPath))
            {
                var json = File.ReadAllText(jsonPath);
                jsonObj = JsonConvert.DeserializeObject<dynamic>(json);
            }
            else
            {
                jsonObj = new JObject();
            }
            if (jsonObj["SavePaths"] == null)
                jsonObj["SavePaths"] = new JObject();
            jsonObj["SavePaths"]["Witcher2"] = folderPath;
            File.WriteAllText(jsonPath, JsonConvert.SerializeObject(jsonObj, Formatting.Indented));
        }

        private void SelectCustomSaveFolder()
        {
            var dlg = new CommonOpenFileDialog();
            dlg.IsFolderPicker = true;
            dlg.Title = "Select Witcher 2 Save Folder";
            dlg.EnsurePathExists = true;
            if (dlg.ShowDialog() == CommonFileDialogResult.Ok)
            {
                var selectedPath = dlg.FileName;
                var hasSaveFiles = Directory.EnumerateFiles(selectedPath, "*.save", SearchOption.TopDirectoryOnly).Any();
                if (!hasSaveFiles)
                {
                    StatusMessage = Utils.ResourceHelper.GetString("Status_NoSaveFiles");
                    return;
                }
                CustomSaveFolderPath = selectedPath;
                UpdateUserPathsJson(selectedPath);

                StatusMessage = Utils.ResourceHelper.GetString("Status_CustomSaveFolderSet");
            }
        }

        private void DeleteSelectedSaves()
        {
            try
            {
                var selected = Saves.Where(s => s.IsSelected).ToList();
                foreach (var save in selected.ToList())
                {
                    var deleteResult = _saveFileService.DeleteSaveFile(save.SaveFile);
                    if (deleteResult)
                    {
                        Saves.Remove(save);
                        StatusMessage = Utils.ResourceHelper.GetString("Status_SaveDeleted");
                    }
                }

                // Update counters after deletion
                TotalSaveFiles = Saves.Count;
                BackedUpFiles = _saveFileService.GetBackupFileCount();

                // Use localized status message format
                StatusMessage = Utils.ResourceHelper.GetFormattedString("Status_DeleteComplete", selected.Count);
            }
            catch (Exception ex)
            {
                StatusMessage = Utils.ResourceHelper.GetFormattedString("Error_DeletingSaves", ex.Message);
            }
        }

        private void SelectBackupFolder()
        {
            var dlg = new CommonOpenFileDialog();
            dlg.IsFolderPicker = true;
            dlg.Title = "Select Backup Folder";
            dlg.EnsurePathExists = true;
            if (dlg.ShowDialog() == CommonFileDialogResult.Ok)
            {
                BackupFilePath = dlg.FileName;

                // Update the service's backup folder and refresh counter
                if (_saveFileService != null)
                {
                    _saveFileService.UpdateBackupFolder(dlg.FileName);
                    UpdateBackupFileCounter();
                }

                StatusMessage = Utils.ResourceHelper.GetString("Status_BackupFolderSet");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
