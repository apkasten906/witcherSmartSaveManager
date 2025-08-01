using System.Collections.Generic;

namespace WitcherCore.Models
{
    /// <summary>
    /// Represents a quest and its current state in a Witcher 2 save file
    /// </summary>
    public class QuestState
    {
        public string QuestId { get; set; } = string.Empty;
        public string QuestName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty; // "Active", "Completed", "Failed", etc.
        public string CurrentPhase { get; set; } = string.Empty;
        public List<string> CompletedObjectives { get; set; } = new();
        public Dictionary<string, object> QuestVariables { get; set; } = new();

        /// <summary>
        /// Key decisions made in this quest that affect the story
        /// </summary>
        public List<QuestDecision> Decisions { get; set; } = new();
    }
}
