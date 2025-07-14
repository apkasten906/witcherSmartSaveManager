using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using WitcherGuiApp.Services;

namespace WitcherGuiApp.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly WitcherSaveFileService _saveFileService;
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

            _saveFileService = new WitcherSaveFileService("Witcher2");
            DeleteSelectedCommand = new RelayCommand(_ => DeleteSelectedSaves(), _ => Saves.Any(s => s.IsSelected));
        }

        public ICommand DeleteSelectedCommand { get; }

        private void Save_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(SaveFileViewModel.IsSelected))
            {
                Application.Current.Dispatcher.Invoke(() =>
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
