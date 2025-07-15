using NUnit.Framework;
using System.Diagnostics.CodeAnalysis;
using WitcherGuiApp.Models;
using WitcherGuiApp.Utils;

[TestFixture]
public class ConfigTests
{
    [TestCase(GameKey.Witcher1, "The Witcher\\saves")]
    [TestCase(GameKey.Witcher2, "Witcher 2\\gamesaves")]
    [TestCase(GameKey.Witcher3, "The Witcher 3\\gamesaves")]
    public void Should_Load_SavePath_For_Game(GameKey gameKey, string exepectedPath)
    {
        string path = SavePathResolver.GetSavePath(gameKey);
        Assert.That(string.IsNullOrWhiteSpace(path), Is.False);
        Assert.That(path, Does.EndWith(exepectedPath));
    }

    //[TestCase("Witcher1", "*.TheWitcherSave")]
    //[TestCase("Witcher2", "*.sav")]
    //[TestCase("Witcher3", "*.sav")]
    //public void Should_Load_SaveExtension_For_Game(string gameKey, string expectedExtension)
    //{
    //    var ext = GameSaveExtensions.GetExtensionForGame(gameKey);
    //    Assert.That(ext, Is.EqualTo(expectedExtension));
    //}
}