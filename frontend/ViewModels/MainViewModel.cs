using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using WitcherGuiApp.Models;
using WitcherGuiApp.Services;

namespace WitcherGuiApp.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly Witcher2Saves _apiClient = new(new System.Net.Http.HttpClient());

        public ObservableCollection<SaveFile> Saves { get; set; } = new();
        
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

        public async Task LoadSavesAsync()
        {
            StatusMessage = "Fetching save files...";
            Saves.Clear();

            try
            {
                var saves = await _apiClient.GetWitcher2SavesAsync() as List<SaveFile>;

                if (saves.Count == 0)
                {
                    StatusMessage = "No save files found.";
                }
                else
                {
                    StatusMessage = $"{saves.Count} save(s) found.";
                }

                foreach (var save in saves)
                {
                    Saves.Add(save);
                }
            }
            catch (System.Exception ex)
            {
                StatusMessage = $"Error: {ex.Message}";
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
