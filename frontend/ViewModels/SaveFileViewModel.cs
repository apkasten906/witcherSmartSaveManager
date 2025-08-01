using Microsoft.VisualBasic;
using System.ComponentModel;
using System.Windows.Input;
using WitcherCore.Models;
using System.Linq;
using System.Collections.Generic;

namespace WitcherSmartSaveManager.ViewModels
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

        // Enhanced metadata properties from database
        public bool HasDatabaseMetadata => SaveFile.Metadata.ContainsKey("database_enhanced");

        public string CurrentQuest
        {
            get
            {
                if (SaveFile.Metadata.TryGetValue("active_quest", out var activeQuest) && activeQuest != null)
                {
                    var quest = activeQuest as dynamic;
                    return quest?.name?.ToString() ?? "Unknown Quest";
                }

                // Fallback to basic metadata
                if (SaveFile.Metadata.TryGetValue("quest", out var basicQuest))
                {
                    return basicQuest.ToString();
                }

                return "No Quest Data";
            }
        }

        public int QuestCount
        {
            get
            {
                if (SaveFile.Metadata.TryGetValue("quest_count", out var count))
                {
                    return (int)count;
                }
                return 0;
            }
        }

        public string QuestDisplay => QuestCount > 0 ? $"{CurrentQuest} ({QuestCount} total)" : "No Quest Data";

        public int CharacterVariableCount
        {
            get
            {
                if (SaveFile.Metadata.TryGetValue("character_variable_count", out var count))
                {
                    return (int)count;
                }
                return 0;
            }
        }

        public string MetadataStatus
        {
            get
            {
                if (HasDatabaseMetadata)
                {
                    return $"Enhanced ({QuestCount} quests, {CharacterVariableCount} vars)";
                }
                else if (SaveFile.Metadata.Count > 0)
                {
                    return "Basic metadata";
                }
                return "No metadata";
            }
        }

        public string GameState
        {
            get
            {
                if (SaveFile.Metadata.TryGetValue("game_state", out var state))
                {
                    return state.ToString();
                }
                return SaveFile.Game.ToString();
            }
        }

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
