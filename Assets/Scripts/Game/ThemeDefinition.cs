using UnityEngine;

namespace TicTacToe.Game
{
    [CreateAssetMenu(menuName = "TicTacToe/Theme Definition", fileName = "ThemeDefinition")]
    public sealed class ThemeDefinition : ScriptableObject
    {
        public string themeName = "Default";

        [Header("Marks")]
        public Sprite xSprite;
        public Sprite oSprite;
        public Color xColor = Color.white;
        public Color oColor = Color.white;

        [Header("Board")]
        public Sprite cellSprite;
        public Color cellColor = new Color(1f, 1f, 1f, 0.08f);
        public Sprite strikeSprite;
        public Color strikeColor = new Color(1f, 1f, 1f, 0.65f);

        [Header("VFX Particles (theme-specific)")]
        public Sprite particleA;
        public Sprite particleB;
    }
}

