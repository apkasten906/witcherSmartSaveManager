using System;
using System.Data.SQLite;
using System.IO;
using System.Xml;

class MigrateXmlToDb
{
    static void Main(string[] args)
    {
        string dbPath = @"C:\Development\witcherSmartSaveManager\database\witcher_save_manager.db";
        string xmlPath = @"C:\Development\witcherSmartSaveManager\savesAnalysis\strings\en0.xml";

        // Ensure database exists
        if (!File.Exists(dbPath))
        {
            Console.WriteLine("Database not found. Please initialize it first.");
            return;
        }

        // Connect to SQLite database
        using (var connection = new SQLiteConnection($"Data Source={dbPath}"))
        {
            connection.Open();

            // Prepare insert command
            var command = connection.CreateCommand();
            command.CommandText = "INSERT INTO LanguageResources (Key, Value, Language) VALUES (@key, @value, @language)";

            // Load XML file
            var doc = new XmlDocument();
            doc.Load(xmlPath);

            foreach (XmlNode node in doc.SelectNodes("//key"))
            {
                string key = node.Attributes["id"]?.Value;
                string value = node.InnerText;

                Console.WriteLine($"Key: {key}, Value: {value}");

                if (!string.IsNullOrEmpty(key) && !string.IsNullOrEmpty(value))
                {
                    command.Parameters.Clear();
                    command.Parameters.AddWithValue("@key", key);
                    command.Parameters.AddWithValue("@value", value);
                    command.Parameters.AddWithValue("@language", "en");

                    command.ExecuteNonQuery();
                }
            }

            Console.WriteLine("XML data migrated successfully.");
        }
    }
}
