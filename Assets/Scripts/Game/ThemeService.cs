using UnityEngine;

namespace TicTacToe.Game
{
    public sealed class ThemeService
    {
        private const string PlayerPrefsKey = "tictactoe.themeName.v1";

        public string LoadThemeName() => PlayerPrefs.GetString(PlayerPrefsKey, string.Empty);

        public void SaveThemeName(string themeName)
        {
            PlayerPrefs.SetString(PlayerPrefsKey, themeName ?? string.Empty);
            PlayerPrefs.Save();
        }
    }
}

