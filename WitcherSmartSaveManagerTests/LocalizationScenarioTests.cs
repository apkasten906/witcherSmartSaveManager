using NUnit.Framework;
using System.Globalization;
using System.Resources;
using WitcherSmartSaveManager;

namespace WitcherSmartSaveManager.Tests
{
    [TestFixture]
    public class LocalizationScenarioTests
    {
        [Test]
        public void SwitchingLanguage_UpdatesResourceStrings()
        {
            var rm = new ResourceManager("WitcherSmartSaveManager.Resources.Strings", typeof(App).Assembly);
            var english = rm.GetString("FindSaves", new CultureInfo("en"));
            var german = rm.GetString("FindSaves", new CultureInfo("de"));
            Assert.That(english, Is.EqualTo("Find Witcher Save Games"));
            Assert.That(german, Is.EqualTo("Witcher Speicherstände finden"));
        }

        [Test]
        public void MissingResourceKey_FallbacksGracefully()
        {
            var rm = new ResourceManager("WitcherSmartSaveManager.Resources.Strings", typeof(App).Assembly);
            var missing = rm.GetString("NonExistentKey", new CultureInfo("en"));
            Assert.That(missing, Is.Null.Or.Empty);
        }
    }
}
