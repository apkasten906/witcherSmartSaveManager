using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;

namespace WitcherSmartSaveManager.Services
{
    public class LocalizationParser
    {
        public Dictionary<string, string> ParseLocalizationFile(string filePath)
        {
            var localizationData = new Dictionary<string, string>();

            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"Localization file not found: {filePath}");
            }

            var document = XDocument.Load(filePath);

            foreach (var element in document.Descendants("string"))
            {
                var id = element.Attribute("id")?.Value;
                var value = element.Value;

                if (!string.IsNullOrEmpty(id) && !string.IsNullOrEmpty(value))
                {
                    localizationData[id] = value;
                }
            }

            return localizationData;
        }
    }
}