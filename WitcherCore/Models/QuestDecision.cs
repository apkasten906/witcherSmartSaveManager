namespace WitcherCore.Models
{
    /// <summary>
    /// Represents a significant decision made during a quest
    /// This is the core of our "smart" save manager functionality
    /// </summary>
    public class QuestDecision
    {
        public string DecisionId { get; set; } = string.Empty;
        public string QuestId { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Choice { get; set; } = string.Empty;

        /// <summary>
        /// Impact level: "Minor", "Major", "Critical"
        /// Used for highlighting important decision points
        /// </summary>
        public string ImpactLevel { get; set; } = "Minor";

        /// <summary>
        /// Categories this decision affects (e.g., "Romance", "Politics", "Character_Fate")
        /// </summary>
        public string[] AffectedCategories { get; set; } = new string[0];
    }
}
