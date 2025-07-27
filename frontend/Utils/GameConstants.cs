using System;

namespace WitcherGuiApp.Utils
{
    public static class GameConstants
    {
        public const string Witcher2DefaultSavePath = @"%userpath%\Documents\Witcher 2\gamesaves";
        public const string Witcher1DefaultSavePath = @"%userpath%\Documents\Witcher\saves";
        public const string Witcher3DefaultSavePath = @"%userpath%\Documents\The Witcher 3\gamesaves";
        public enum GameIdentifier { Witcher1, Witcher2, Witcher3 }
        public enum Source { Steam, GOG }
    }
}
