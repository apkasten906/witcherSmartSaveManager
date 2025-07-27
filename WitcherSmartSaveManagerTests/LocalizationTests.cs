using NUnit.Framework;
using System.Globalization;
using System.Resources;

namespace WitcherGuiApp.Tests
{
    [TestFixture]
    public class LocalizationTests
    {
        [Test]
        public void EnglishResourceFile_ContainsExpectedStrings()
        {
            var rm = new ResourceManager("WitcherGuiApp.Resources.Strings", typeof(App).Assembly);
            Assert.That(rm.GetString("AppTitle", new CultureInfo("en")), Is.EqualTo("Witcher Smart Save Manager"));
            Assert.That(rm.GetString("FindSaves", new CultureInfo("en")), Is.EqualTo("Find Witcher Save Games"));
        }

        [Test]
        public void GermanResourceFile_ContainsExpectedStrings()
        {
            var rm = new ResourceManager("WitcherGuiApp.Resources.Strings", typeof(App).Assembly);
            Assert.That(rm.GetString("FindSaves", new CultureInfo("de")), Is.EqualTo("Witcher Speicherstände finden"));
        }
    }
}
