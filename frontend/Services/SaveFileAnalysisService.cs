using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace WitcherSmartSaveManager.Services
{
    public class SaveFileAnalysisService
    {
        private readonly string _toolPath;
        private readonly string _backupPath;
        private readonly string _cookedPCPath;

        public SaveFileAnalysisService(string toolPath, string backupPath, string cookedPCPath)
        {
            _toolPath = toolPath;
            _backupPath = backupPath;
            _cookedPCPath = cookedPCPath;
        }

        public void ExtractStrings(string inputFile, string outputFile)
        {
            RunTool("Gibbed.RED.Strings.exe", $"-d -f \"{inputFile}\" \"{outputFile}\"");
        }

        public void UnpackDzip(string inputFile, string outputDirectory)
        {
            RunTool("Gibbed.RED.Unpack.exe", $"\"{inputFile}\" \"{outputDirectory}\"");
        }

        public List<SaveFileMetadata> AnalyzeSaveFiles()
        {
            var saveFiles = Directory.GetFiles(_backupPath, "*.sav");
            var metadataList = new List<SaveFileMetadata>();

            foreach (var saveFile in saveFiles)
            {
                // Placeholder: Add logic to parse save files and extract metadata
                metadataList.Add(new SaveFileMetadata
                {
                    FileName = Path.GetFileName(saveFile),
                    LastModified = File.GetLastWriteTime(saveFile),
                    QuestName = "Unknown", // Placeholder
                    QuestPhase = "Unknown", // Placeholder
                    Variables = new Dictionary<string, bool>() // Placeholder
                });
            }

            return metadataList;
        }

        private void RunTool(string toolName, string arguments)
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = Path.Combine(_toolPath, toolName),
                    Arguments = arguments,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            process.Start();
            string output = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();
            process.WaitForExit();

            if (process.ExitCode != 0)
            {
                throw new Exception($"Error running {toolName}: {error}");
            }

            Console.WriteLine(output);
        }
    }

    public class SaveFileMetadata
    {
        public string FileName { get; set; }
        public DateTime LastModified { get; set; }
        public string QuestName { get; set; }
        public string QuestPhase { get; set; }
        public Dictionary<string, bool> Variables { get; set; }
    }
}