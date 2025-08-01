using System;
using System.Collections.Generic;

namespace WitcherCore.Models
{
    /// <summary>
    /// Complete data structure for a parsed Witcher 2 save file
    /// </summary>
    public class SaveFileData
    {
        public SaveFileHeader Header { get; set; } = new();
        public List<QuestState> Quests { get; set; } = new();
        public Dictionary<string, object> CharacterVariables { get; set; } = new();
        public List<InventoryItem> Inventory { get; set; } = new();
        public ParseResult ParseResult { get; set; } = new();
    }
}
