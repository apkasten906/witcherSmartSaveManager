namespace WitcherCore.Models
{
    /// <summary>
    /// Result of parsing a save file, including success status and error information
    /// </summary>
    public class ParseResult
    {
        public bool Success { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
        public string WarningMessage { get; set; } = string.Empty;

        /// <summary>
        /// Parsing confidence level (0-100)
        /// Lower values indicate uncertain or partial parsing
        /// </summary>
        public int ConfidenceLevel { get; set; } = 100;
    }
}
