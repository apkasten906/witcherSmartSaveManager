using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using Microsoft.Extensions.Configuration;
using NLog;
using WitcherCore.Models;
using System.Threading.Tasks;
using System.Linq;

namespace WitcherCore.Services
{
    /// <summary>
    /// Service for managing enhanced save file metadata in the database
    /// Implements the hybrid approach: file-based core + database enhancements
    /// </summary>
    public class SaveFileMetadataService
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly string _connectionString;

        public SaveFileMetadataService()
        {
            // Get connection string from App.config
            var config = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: true)
                .Build();

            _connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["DatabaseConnectionString"]?.ConnectionString
                ?? "Data Source=witcher_save_manager.db;Version=3;Journal Mode=WAL;Cache Size=10000;Synchronous=Normal;";
        }

        /// <summary>
        /// Inserts or updates save file metadata in the database
        /// </summary>
        public async Task<bool> UpsertSaveFileMetadataAsync(WitcherSaveFile saveFile)
        {
            try
            {
                using var connection = new SQLiteConnection(_connectionString);
                await connection.OpenAsync();

                // First, ensure we have a SaveFiles entry
                var saveFileId = await EnsureSaveFileExistsAsync(connection, saveFile);

                // Then insert/update the enhanced metadata
                const string upsertSql = @"
                    INSERT OR REPLACE INTO SaveFileMetadata 
                    (SaveFileId, FileName, GameKey, FullPath, ScreenshotPath, FileSize, LastModified, ModifiedTimeIso, UpdatedAt)
                    VALUES (@SaveFileId, @FileName, @GameKey, @FullPath, @ScreenshotPath, @FileSize, @LastModified, @ModifiedTimeIso, @UpdatedAt)";

                using var command = new SQLiteCommand(upsertSql, connection);
                command.Parameters.AddWithValue("@SaveFileId", saveFileId);
                command.Parameters.AddWithValue("@FileName", saveFile.FileName);
                command.Parameters.AddWithValue("@GameKey", saveFile.Game.ToString());
                command.Parameters.AddWithValue("@FullPath", saveFile.FullPath);
                command.Parameters.AddWithValue("@ScreenshotPath", saveFile.ScreenshotPath ?? string.Empty);
                command.Parameters.AddWithValue("@FileSize", saveFile.Size);
                command.Parameters.AddWithValue("@LastModified", DateTime.UtcNow); // TODO: Use actual file time
                command.Parameters.AddWithValue("@ModifiedTimeIso", saveFile.ModifiedTimeIso);
                command.Parameters.AddWithValue("@UpdatedAt", DateTime.UtcNow);

                await command.ExecuteNonQueryAsync();

                Logger.Debug($"Upserted metadata for save file: {saveFile.FileName}");
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"Failed to upsert metadata for save file: {saveFile.FileName}");
                return false;
            }
        }

        /// <summary>
        /// Retrieves enhanced metadata for a save file from the database
        /// </summary>
        public async Task<Dictionary<string, object>> GetEnhancedMetadataAsync(string fileName)
        {
            var metadata = new Dictionary<string, object>();

            try
            {
                using var connection = new SQLiteConnection(_connectionString);
                await connection.OpenAsync();

                // Get basic metadata
                const string metadataSql = @"
                    SELECT svm.*, sf.GameState
                    FROM SaveFileMetadata svm
                    LEFT JOIN SaveFiles sf ON sf.Id = svm.SaveFileId
                    WHERE svm.FileName = @FileName";

                using var command = new SQLiteCommand(metadataSql, connection);
                command.Parameters.AddWithValue("@FileName", fileName);

                using var reader = await command.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    metadata["database_enhanced"] = true;
                    metadata["game_key"] = reader["GameKey"]?.ToString() ?? string.Empty;
                    metadata["file_size"] = reader["FileSize"];
                    metadata["last_modified"] = reader["LastModified"]?.ToString() ?? string.Empty;
                    metadata["screenshot_path"] = reader["ScreenshotPath"]?.ToString() ?? string.Empty;
                    metadata["game_state"] = reader["GameState"]?.ToString() ?? string.Empty;
                }

                // Get quest information if available
                await LoadQuestDataAsync(connection, fileName, metadata);

                // Get character variables if available  
                await LoadCharacterDataAsync(connection, fileName, metadata);

                Logger.Debug($"Retrieved enhanced metadata for: {fileName}");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"Failed to retrieve enhanced metadata for: {fileName}");
                metadata["database_error"] = ex.Message;
            }

            return metadata;
        }

        /// <summary>
        /// Stores parsed quest information for a Witcher 2 save file
        /// </summary>
        public async Task<bool> StoreQuestDataAsync(string fileName, List<QuestInfo> quests)
        {
            try
            {
                using var connection = new SQLiteConnection(_connectionString);
                await connection.OpenAsync();

                // Get the SaveFileMetadata ID
                var metadataId = await GetSaveFileMetadataIdAsync(connection, fileName);
                if (metadataId == null)
                {
                    Logger.Warn($"No metadata found for {fileName}, cannot store quest data");
                    return false;
                }

                // Clear existing quest data
                const string deleteSql = "DELETE FROM QuestInfo WHERE SaveFileId = @SaveFileId";
                using var deleteCommand = new SQLiteCommand(deleteSql, connection);
                deleteCommand.Parameters.AddWithValue("@SaveFileId", metadataId);
                await deleteCommand.ExecuteNonQueryAsync();

                // Insert new quest data
                const string insertSql = @"
                    INSERT INTO QuestInfo (SaveFileId, QuestName, QuestPhase, QuestDescription, IsCompleted)
                    VALUES (@SaveFileId, @QuestName, @QuestPhase, @QuestDescription, @IsCompleted)";

                foreach (var quest in quests)
                {
                    using var insertCommand = new SQLiteCommand(insertSql, connection);
                    insertCommand.Parameters.AddWithValue("@SaveFileId", metadataId);
                    insertCommand.Parameters.AddWithValue("@QuestName", quest.QuestName);
                    insertCommand.Parameters.AddWithValue("@QuestPhase", quest.QuestPhase);
                    insertCommand.Parameters.AddWithValue("@QuestDescription", quest.QuestDescription);
                    insertCommand.Parameters.AddWithValue("@IsCompleted", quest.IsCompleted);
                    await insertCommand.ExecuteNonQueryAsync();
                }

                Logger.Info($"Stored {quests.Count} quests for save file: {fileName}");
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"Failed to store quest data for: {fileName}");
                return false;
            }
        }

        #region Private Helper Methods

        private async Task<int> EnsureSaveFileExistsAsync(SQLiteConnection connection, WitcherSaveFile saveFile)
        {
            // Check if SaveFile exists
            const string checkSql = "SELECT Id FROM SaveFiles WHERE FileName = @FileName";
            using var checkCommand = new SQLiteCommand(checkSql, connection);
            checkCommand.Parameters.AddWithValue("@FileName", saveFile.FileName);

            var existingId = await checkCommand.ExecuteScalarAsync();
            if (existingId != null)
            {
                return Convert.ToInt32(existingId);
            }

            // Insert new SaveFile
            const string insertSql = @"
                INSERT INTO SaveFiles (FileName, Timestamp, GameState) 
                VALUES (@FileName, @Timestamp, @GameState);
                SELECT last_insert_rowid();";

            using var insertCommand = new SQLiteCommand(insertSql, connection);
            insertCommand.Parameters.AddWithValue("@FileName", saveFile.FileName);
            insertCommand.Parameters.AddWithValue("@Timestamp", DateTime.UtcNow); // TODO: Use actual file time
            insertCommand.Parameters.AddWithValue("@GameState", saveFile.Game.ToString());

            var newId = await insertCommand.ExecuteScalarAsync();
            return Convert.ToInt32(newId);
        }

        private async Task<int?> GetSaveFileMetadataIdAsync(SQLiteConnection connection, string fileName)
        {
            const string sql = "SELECT Id FROM SaveFileMetadata WHERE FileName = @FileName";
            using var command = new SQLiteCommand(sql, connection);
            command.Parameters.AddWithValue("@FileName", fileName);

            var result = await command.ExecuteScalarAsync();
            return result != null ? Convert.ToInt32(result) : null;
        }

        private async Task LoadQuestDataAsync(SQLiteConnection connection, string fileName, Dictionary<string, object> metadata)
        {
            const string questSql = @"
                SELECT qi.QuestName, qi.QuestPhase, qi.QuestDescription, qi.IsCompleted
                FROM QuestInfo qi
                INNER JOIN SaveFileMetadata svm ON qi.SaveFileId = svm.Id
                WHERE svm.FileName = @FileName
                ORDER BY qi.QuestName";

            using var command = new SQLiteCommand(questSql, connection);
            command.Parameters.AddWithValue("@FileName", fileName);

            var quests = new List<object>();
            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                quests.Add(new
                {
                    name = reader["QuestName"]?.ToString(),
                    phase = reader["QuestPhase"]?.ToString(),
                    description = reader["QuestDescription"]?.ToString(),
                    completed = Convert.ToBoolean(reader["IsCompleted"])
                });
            }

            if (quests.Count > 0)
            {
                metadata["quests"] = quests;
                metadata["quest_count"] = quests.Count;
                var activeQuest = quests.FirstOrDefault(q => !((dynamic)q).completed);
                if (activeQuest != null)
                {
                    metadata["active_quest"] = activeQuest;
                }
            }
        }

        private async Task LoadCharacterDataAsync(SQLiteConnection connection, string fileName, Dictionary<string, object> metadata)
        {
            const string characterSql = @"
                SELECT cv.VariableName, cv.VariableValue, cv.VariableType
                FROM CharacterVariables cv
                INNER JOIN SaveFileMetadata svm ON cv.SaveFileId = svm.Id
                WHERE svm.FileName = @FileName
                ORDER BY cv.VariableName";

            using var command = new SQLiteCommand(characterSql, connection);
            command.Parameters.AddWithValue("@FileName", fileName);

            var variables = new Dictionary<string, object>();
            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var name = reader["VariableName"]?.ToString();
                var value = reader["VariableValue"]?.ToString();
                var type = reader["VariableType"]?.ToString();

                if (!string.IsNullOrEmpty(name))
                {
                    variables[name] = new { value, type };
                }
            }

            if (variables.Count > 0)
            {
                metadata["character_variables"] = variables;
                metadata["character_variable_count"] = variables.Count;
            }
        }

        #endregion
    }

    /// <summary>
    /// Quest information model for database operations
    /// </summary>
    public class QuestInfo
    {
        public string QuestName { get; set; } = string.Empty;
        public string QuestPhase { get; set; } = string.Empty;
        public string QuestDescription { get; set; } = string.Empty;
        public bool IsCompleted { get; set; }
    }
}
