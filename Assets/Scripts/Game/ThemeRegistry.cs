using System;
using System.Collections.Generic;
using UnityEngine;

namespace TicTacToe.Game
{
    public sealed class ThemeRegistry : MonoBehaviour
    {
        [SerializeField] private List<ThemeDefinition> themes = new List<ThemeDefinition>();

        public IReadOnlyList<ThemeDefinition> Themes => themes;

        public ThemeDefinition GetByNameOrDefault(string themeName)
        {
            if (!string.IsNullOrWhiteSpace(themeName))
            {
                foreach (var t in themes)
                {
                    if (t != null && string.Equals(t.themeName, themeName, StringComparison.OrdinalIgnoreCase))
                        return t;
                }
            }

            foreach (var t in themes)
            {
                if (t != null)
                    return t;
            }

            return null;
        }
    }
}

