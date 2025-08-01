using System;
using WitcherCore.Tools;
using NLog;

namespace WitcherCore
{
    /// <summary>
    /// Console application for analyzing Witcher 2 save file format
    /// Phase 1: Structure Discovery
    /// </summary>
    class SaveAnalysisProgram
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        static void Main(string[] args)
        {
            Console.WriteLine("🐺 Witcher 2 Save File Format Analyzer");
            Console.WriteLine("=====================================");
            Console.WriteLine("Phase 1: Structure Discovery");
            Console.WriteLine();

            try
            {
                var tool = new SaveAnalysisTool();

                if (args.Length > 0)
                {
                    // Analyze specific file
                    tool.AnalyzeSpecificFile(args[0]);
                }
                else
                {
                    // Analyze all backup saves
                    tool.AnalyzeBackupSaves();
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Analysis failed");
                Console.WriteLine($"❌ Error during analysis: {ex.Message}");
                Console.WriteLine();
                Console.WriteLine("Stack trace:");
                Console.WriteLine(ex.StackTrace);
            }

            Console.WriteLine();
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }
}
