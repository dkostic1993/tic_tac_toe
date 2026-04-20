using System;
using System.Linq;
using TicTacToe.Game;
using UnityEditor;
using UnityEngine;

namespace TicTacToe.Editor
{
    public static class TicTacToeThemeAutoAssigner
    {
        private const string ThemeAssetPath = "Assets/ScriptableObjects/Theme_Default.asset";
        private const string XoPsdPath = "Assets/Needs/Art/XO.psd";

        [MenuItem("Tools/TicTacToe/Auto Assign Theme Sprites")]
        public static void AutoAssign()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                Debug.LogWarning("TicTacToe: Stop Play mode before auto-assigning theme sprites.");
                return;
            }

            EnsurePsdImportedAsSprites();

            var theme = AssetDatabase.LoadAssetAtPath<ThemeDefinition>(ThemeAssetPath);
            if (theme == null)
            {
                Debug.LogError($"TicTacToe: Missing theme asset at '{ThemeAssetPath}'. Run scene generator first.");
                return;
            }

            var sprites = AssetDatabase.LoadAllAssetsAtPath(XoPsdPath).OfType<Sprite>().ToArray();
            if (sprites.Length == 0)
            {
                Debug.LogWarning($"TicTacToe: No sprites found inside '{XoPsdPath}'. If PSD Importer is not installed, export PNG sprites and assign manually.");
                return;
            }

            static float Area(Sprite s) => s == null ? float.PositiveInfinity : s.rect.width * s.rect.height;
            static float Aspect(Sprite s)
            {
                if (s == null) return 0f;
                var w = Mathf.Max(1f, s.rect.width);
                var h = Mathf.Max(1f, s.rect.height);
                return w > h ? (w / h) : (h / w);
            }

            static bool HasAnyToken(string haystack, params string[] tokens)
                => tokens.Any(t => haystack.IndexOf(t, StringComparison.OrdinalIgnoreCase) >= 0);

            Sprite[] ByTokens(Func<Sprite, bool> extraFilter, params string[] tokens) =>
                sprites.Where(s => HasAnyToken(s.name, tokens) && (extraFilter == null || extraFilter(s))).ToArray();

            Sprite PickSmallest(Sprite[] candidates)
                => candidates.OrderBy(Area).FirstOrDefault(s => Area(s) < 256f * 256f); // avoid picking full atlas

            Sprite PickLargest(Sprite[] candidates)
                => candidates.OrderByDescending(Area).FirstOrDefault();

            Sprite PickStrike(Sprite[] candidates)
                => candidates
                    .OrderByDescending(Aspect)
                    .ThenByDescending(Area)
                    .FirstOrDefault(s => Aspect(s) >= 3.0f);

            // Best-effort name matching; depends on PSD layer/sprite names.
            bool RejectAmbiguousXO(Sprite s)
            {
                var n = s.name;
                // Avoid sprites whose names imply they include both marks or are an atlas.
                if (HasAnyToken(n, "xo", "x_o", "x-o", "atlas", "sheet", "all", "set", "icons"))
                    return false;
                return true;
            }

            var x = PickSmallest(ByTokens(RejectAmbiguousXO, "x", "cross"));
            var o = PickSmallest(ByTokens(RejectAmbiguousXO, "o", "circle", "nought"));
            var cell = PickLargest(ByTokens(null, "cell", "tile", "square", "board", "grid"));
            var strike = PickStrike(ByTokens(null, "strike", "line", "win"));

            // If we accidentally picked the same sprite for both, prefer fallback text by clearing them.
            if (x != null && o != null && x == o)
            {
                x = null;
                o = null;
            }

            Undo.RecordObject(theme, "Auto Assign Theme Sprites");
            theme.xSprite = x;
            theme.oSprite = o;
            if (cell != null) theme.cellSprite = cell;
            if (strike != null) theme.strikeSprite = strike;
            EditorUtility.SetDirty(theme);
            AssetDatabase.SaveAssets();

            Debug.Log(
                "TicTacToe: Theme sprites assigned.\n" +
                $"- FoundSprites: {sprites.Length}\n" +
                $"- X: {(theme.xSprite != null ? theme.xSprite.name : "<fallback-text>")}\n" +
                $"- O: {(theme.oSprite != null ? theme.oSprite.name : "<fallback-text>")}\n" +
                $"- Cell: {(theme.cellSprite != null ? theme.cellSprite.name : "<none>")}\n" +
                $"- Strike: {(theme.strikeSprite != null ? theme.strikeSprite.name : "<none>")}"
            );
        }

        [MenuItem("Tools/TicTacToe/Auto Assign Theme Sprites", true)]
        private static bool AutoAssign_Validate() => !EditorApplication.isPlayingOrWillChangePlaymode;

        private static void EnsurePsdImportedAsSprites()
        {
            var importer = AssetImporter.GetAtPath(XoPsdPath);
            if (importer == null)
            {
                Debug.LogWarning($"TicTacToe: Can't find '{XoPsdPath}'.");
                return;
            }

            // If PSD Importer is installed, its importer type is UnityEditor.U2D.PSD.PSDImporter.
            // We use reflection so this file still compiles even if the package is missing.
            var psdImporterType = Type.GetType("UnityEditor.U2D.PSD.PSDImporter, Unity.2D.PSDImporter.Editor");
            if (psdImporterType == null || !psdImporterType.IsInstanceOfType(importer))
            {
                // Fallback: ensure it's imported as a sprite (single) so you can still use it manually.
                if (importer is TextureImporter tex)
                {
                    var changed = false;
                    if (tex.textureType != TextureImporterType.Sprite) { tex.textureType = TextureImporterType.Sprite; changed = true; }
                    if (tex.spriteImportMode != SpriteImportMode.Single) { tex.spriteImportMode = SpriteImportMode.Single; changed = true; }
                    if (changed) tex.SaveAndReimport();
                }
                return;
            }

            // PSDImporter has various serialized settings; we just force a reimport to ensure sprites exist.
            importer.SaveAndReimport();
        }
    }
}

