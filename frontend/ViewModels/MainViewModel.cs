using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using WitcherGuiApp.Models;
using WitcherGuiApp.Services;
using WitcherGuiApp.ViewModels;

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

        private bool _areAllSelected;
        public bool AreAllSelected
        {
            get => _areAllSelected;
            set
            {
                if (_areAllSelected != value)
                {
                    _areAllSelected = value;
                    OnPropertyChanged();

                    foreach (var save in Saves)
                    {
                        save.IsSelected = value;
                    }
                }
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
                _areAllSelected = Saves.All(s => s.IsSelected);
                OnPropertyChanged(nameof(AreAllSelected));
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
                        var vm = new SaveFileViewModel(save);
                        vm.PropertyChanged += Save_PropertyChanged;
                        App.Current.Dispatcher.Invoke(() => Saves.Add(vm));
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
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
