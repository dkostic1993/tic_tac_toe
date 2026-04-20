using System.Collections.Generic;
using System.IO;
using TicTacToe.Audio;
using TicTacToe.Game;
using TicTacToe.UI;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace TicTacToe.Editor
{
    public static class TicTacToeSceneGenerator
    {
        private const string ScenesDir = "Assets/Scenes";
        private const string ScriptableObjectsDir = "Assets/ScriptableObjects";

        [MenuItem("Tools/TicTacToe/Generate Scenes & Defaults")]
        public static void GenerateAll()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                Debug.LogWarning("TicTacToe: Stop Play mode before generating scenes.");
                return;
            }

            EnsureDirs();
            EnsureDefaultThemeAssets();
            TicTacToeThemeAutoAssigner.AutoAssign();

            // Important: Generate scenes sequentially. NewSceneMode.Single invalidates previous scene handles.
            var play = GeneratePlayScene();
            foreach (var root in play.GetRootGameObjects())
                GameObjectUtility.RemoveMonoBehavioursWithMissingScript(root);
            EditorSceneManager.SaveScene(play, Path.Combine(ScenesDir, $"{SceneNames.Play}.unity"));

            var game = GenerateGameScene();
            foreach (var root in game.GetRootGameObjects())
                GameObjectUtility.RemoveMonoBehavioursWithMissingScript(root);
            EditorSceneManager.SaveScene(game, Path.Combine(ScenesDir, $"{SceneNames.Game}.unity"));

            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(Path.Combine(ScenesDir, $"{SceneNames.Play}.unity"), true),
                new EditorBuildSettingsScene(Path.Combine(ScenesDir, $"{SceneNames.Game}.unity"), true),
            };

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            // Ensure the editor is left in the Play scene so pressing ▶ starts from main menu.
            EditorSceneManager.OpenScene(Path.Combine(ScenesDir, $"{SceneNames.Play}.unity"));

            Debug.Log("TicTacToe: Scenes generated. Press Play to start from main menu.");
        }

        [MenuItem("Tools/TicTacToe/Generate Scenes & Defaults", true)]
        private static bool GenerateAll_Validate()
        {
            return !EditorApplication.isPlayingOrWillChangePlaymode;
        }

        private static void EnsureDirs()
        {
            if (!AssetDatabase.IsValidFolder("Assets/Scenes"))
                AssetDatabase.CreateFolder("Assets", "Scenes");
            if (!AssetDatabase.IsValidFolder("Assets/ScriptableObjects"))
                AssetDatabase.CreateFolder("Assets", "ScriptableObjects");
        }

        private static void EnsureDefaultThemeAssets()
        {
            EnsureTheme(
                assetName: "Theme_Default.asset",
                themeName: "Default",
                x: Color.white,
                o: Color.white,
                cell: new Color(1f, 1f, 1f, 0.08f),
                strike: new Color(1f, 1f, 1f, 0.65f)
            );

            EnsureTheme(
                assetName: "Theme_Neon.asset",
                themeName: "Neon",
                x: new Color(0.2f, 0.85f, 1f, 1f),
                o: new Color(1f, 0.85f, 0.2f, 1f),
                cell: new Color(1f, 1f, 1f, 0.05f),
                strike: new Color(1f, 0.35f, 0.85f, 0.85f)
            );
        }

        private static void EnsureTheme(string assetName, string themeName, Color x, Color o, Color cell, Color strike)
        {
            var themePath = Path.Combine(ScriptableObjectsDir, assetName);
            var theme = AssetDatabase.LoadAssetAtPath<ThemeDefinition>(themePath);
            if (theme == null)
            {
                theme = ScriptableObject.CreateInstance<ThemeDefinition>();
                theme.themeName = themeName;
                theme.xColor = x;
                theme.oColor = o;
                theme.cellColor = cell;
                theme.strikeColor = strike;
                AssetDatabase.CreateAsset(theme, themePath);
            }
            else
            {
                // Keep any user-assigned sprites, but ensure colors exist.
                theme.themeName = themeName;
                theme.xColor = x;
                theme.oColor = o;
                theme.cellColor = cell;
                theme.strikeColor = strike;
                EditorUtility.SetDirty(theme);
            }
        }

        private static Scene GeneratePlayScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            CreateCamera();
            CreateEventSystem();
            var canvas = CreateCanvas("Canvas");
            canvas.gameObject.AddComponent<EnsureCamera>();
            var safeArea = CreateSafeAreaRoot(canvas.transform);

            var audio = CreateAudioManager();
            // AudioManager already calls DontDestroyOnLoad in play mode (its Awake).
            // Calling DontDestroyOnLoad from editor scripts throws InvalidOperationException.

            var themeRegistry = CreateThemeRegistry(safeArea.transform);

            var menuRoot = new GameObject("PlayMenuRoot", typeof(RectTransform));
            menuRoot.transform.SetParent(safeArea.transform, false);
            StretchToParent(menuRoot.GetComponent<RectTransform>());

            var (playButton, statsButton, settingsButton, exitButton) = CreateMainMenuButtons(menuRoot.transform);

            // Popups
            var themePopup = CreateThemeSelectionPopup(safeArea.transform, themeRegistry);
            var statsPopup = CreateStatsPopup(safeArea.transform);
            var settingsPopup = CreateSettingsPopup(safeArea.transform);
            var exitPopup = CreateExitPopup(safeArea.transform);

            var menu = menuRoot.AddComponent<PlayMenuController>();
            SetPrivateField(menu, "playButton", playButton);
            SetPrivateField(menu, "statsButton", statsButton);
            SetPrivateField(menu, "settingsButton", settingsButton);
            SetPrivateField(menu, "exitButton", exitButton);
            SetPrivateField(menu, "themeSelectionPopup", themePopup);
            SetPrivateField(menu, "statsPopup", statsPopup);
            SetPrivateField(menu, "settingsPopup", settingsPopup);
            SetPrivateField(menu, "exitPopup", exitPopup);

            themePopup.HideImmediate();
            statsPopup.HideImmediate();
            settingsPopup.HideImmediate();
            exitPopup.HideImmediate();

            EditorSceneManager.MarkSceneDirty(scene);
            return scene;
        }

        private static Scene GenerateGameScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            CreateCamera();
            CreateEventSystem();
            var canvas = CreateCanvas("Canvas");
            canvas.gameObject.AddComponent<EnsureCamera>();
            var safeArea = CreateSafeAreaRoot(canvas.transform);

            // Ensure audio exists even if user starts Play mode from Game scene.
            CreateAudioManager();

            var themeRegistry = CreateThemeRegistry(safeArea.transform);

            var root = new GameObject("GameRoot", typeof(RectTransform));
            root.transform.SetParent(safeArea.transform, false);
            StretchToParent(root.GetComponent<RectTransform>());

            var boardRoot = new GameObject("Board", typeof(RectTransform));
            boardRoot.transform.SetParent(root.transform, false);
            var boardRect = boardRoot.GetComponent<RectTransform>();
            boardRect.anchorMin = new Vector2(0.5f, 0.5f);
            boardRect.anchorMax = new Vector2(0.5f, 0.5f);
            boardRect.pivot = new Vector2(0.5f, 0.5f);
            boardRect.sizeDelta = new Vector2(620, 620);
            boardRect.anchoredPosition = Vector2.zero;

            var grid = boardRoot.AddComponent<GridLayoutGroup>();
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = 3;
            grid.cellSize = new Vector2(190, 190);
            grid.spacing = new Vector2(12, 12);
            grid.childAlignment = TextAnchor.MiddleCenter;

            var cells = new BoardCellView[BoardState.CellCount];
            for (var i = 0; i < BoardState.CellCount; i++)
            {
                cells[i] = CreateBoardCell(boardRoot.transform, i);
            }

            var strike = CreateStrike(boardRoot.transform);

            var (hud, settingsButton) = CreateHudWithSettings(root.transform);
            var settingsPopup = CreateSettingsPopup(safeArea.transform);
            settingsPopup.HideImmediate();

            var resultPopup = CreateGameResultPopup(safeArea.transform);
            resultPopup.HideImmediate();

            var controller = root.AddComponent<GameController>();
            SetPrivateField(controller, "themeRegistry", themeRegistry);
            SetPrivateField(controller, "cells", cells);
            SetPrivateField(controller, "strikeView", strike);
            SetPrivateField(controller, "hud", hud);
            SetPrivateField(controller, "resultPopup", resultPopup);

            var settingsButtonController = settingsButton.AddComponent<GameSettingsButtonController>();
            SetPrivateField(settingsButtonController, "settingsButton", settingsButton.GetComponent<Button>());
            SetPrivateField(settingsButtonController, "settingsPopup", settingsPopup);

            SetPrivateField(resultPopup, "gameController", controller);

            EditorSceneManager.MarkSceneDirty(scene);
            return scene;
        }

        private static void StretchToParent(RectTransform rect)
        {
            if (rect == null) return;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            rect.pivot = new Vector2(0.5f, 0.5f);
        }

        private static void CreateEventSystem()
        {
            if (Object.FindFirstObjectByType<EventSystem>() != null)
                return;
            new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        }

        private static void CreateCamera()
        {
            if (Object.FindFirstObjectByType<Camera>() != null)
                return;

            var camGo = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
            camGo.tag = "MainCamera";
            var cam = camGo.GetComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.06f, 0.06f, 0.07f, 1f);
            cam.orthographic = true;
            cam.orthographicSize = 5;
        }

        private static Canvas CreateCanvas(string name)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            var scaler = go.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.matchWidthOrHeight = 0.5f;

            var bg = new GameObject("Background", typeof(RectTransform), typeof(Image));
            bg.transform.SetParent(go.transform, false);
            var bgRect = bg.GetComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.offsetMin = Vector2.zero;
            bgRect.offsetMax = Vector2.zero;
            var bgImg = bg.GetComponent<Image>();
            bgImg.color = new Color(0f, 0f, 0f, 0.7f);
            bgImg.raycastTarget = false;

            return canvas;
        }

        private static GameObject CreateSafeAreaRoot(Transform canvas)
        {
            var safe = new GameObject("SafeArea", typeof(RectTransform));
            safe.transform.SetParent(canvas, false);
            var rect = safe.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            var fitter = safe.AddComponent<SafeAreaFitter>();
            SetPrivateField(fitter, "target", rect);

            return safe;
        }

        private static AudioManager CreateAudioManager()
        {
            var go = new GameObject("AudioManager", typeof(AudioManager));
            var mgr = go.GetComponent<AudioManager>();

            var bgm = new GameObject("BGM", typeof(AudioSource));
            bgm.transform.SetParent(go.transform, false);
            var sfx = new GameObject("SFX", typeof(AudioSource));
            sfx.transform.SetParent(go.transform, false);

            var bgmSource = bgm.GetComponent<AudioSource>();
            bgmSource.playOnAwake = false;
            bgmSource.spatialBlend = 0f;
            bgmSource.volume = 0.65f;

            var sfxSource = sfx.GetComponent<AudioSource>();
            sfxSource.playOnAwake = false;
            sfxSource.spatialBlend = 0f;
            sfxSource.volume = 0.9f;

            SetPrivateField(mgr, "bgmSource", bgmSource);
            SetPrivateField(mgr, "sfxSource", sfxSource);

            // Auto-wire provided assignment clips when present in Assets/Audio.
            // Prefer direct paths to avoid AssetDatabase.FindAssets timing/import quirks.
            var music = LoadClipAt("Assets/Audio/music.wav") ?? FindAudioClipByName("music");
            var click1 = LoadClipAt("Assets/Audio/click1.wav") ?? FindAudioClipByName("click1");
            var click2 = LoadClipAt("Assets/Audio/click2.wav") ?? FindAudioClipByName("click2");
            var pop = LoadClipAt("Assets/Audio/pop.wav") ?? FindAudioClipByName("pop");
            var woosh = LoadClipAt("Assets/Audio/woosh.wav") ?? FindAudioClipByName("woosh");

            SetPrivateField(mgr, "bgmClip", music);
            SetPrivateField(mgr, "buttonClickClips", new[] { click1, click2 });
            SetPrivateField(mgr, "popupOpenClip", woosh != null ? woosh : pop);
            SetPrivateField(mgr, "popupCloseClip", pop != null ? pop : woosh);

            // Reuse existing clips if there are no dedicated ones.
            SetPrivateField(mgr, "placeMarkClip", click1);
            SetPrivateField(mgr, "strikeWinClip", pop);

            return mgr;
        }

        private static AudioClip LoadClipAt(string assetPath)
        {
            // Ensure it's imported before we try to load it (prevents nulls on fresh projects).
            AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
            return AssetDatabase.LoadAssetAtPath<AudioClip>(assetPath);
        }

        private static AudioClip FindAudioClipByName(string nameWithoutExt)
        {
            var guids = AssetDatabase.FindAssets($"{nameWithoutExt} t:AudioClip");
            // Prefer Assets/Audio but allow fallback anywhere in Assets.
            AudioClip fallback = null;
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var clip = AssetDatabase.LoadAssetAtPath<AudioClip>(path);
                if (clip != null)
                {
                    if (path.StartsWith("Assets/Audio/"))
                        return clip;
                    fallback ??= clip;
                }
            }
            return fallback;
        }

        private static ThemeRegistry CreateThemeRegistry(Transform parent)
        {
            var go = new GameObject("ThemeRegistry", typeof(ThemeRegistry));
            go.transform.SetParent(parent, false);

            var list = new List<ThemeDefinition>();
            var themeGuids = AssetDatabase.FindAssets("t:ThemeDefinition", new[] { ScriptableObjectsDir });
            foreach (var guid in themeGuids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var theme = AssetDatabase.LoadAssetAtPath<ThemeDefinition>(path);
                if (theme != null)
                    list.Add(theme);
            }
            list.Sort((a, b) => string.Compare(a != null ? a.themeName : "", b != null ? b.themeName : "", System.StringComparison.OrdinalIgnoreCase));

            var registry = go.GetComponent<ThemeRegistry>();
            SetPrivateField(registry, "themes", list);
            return registry;
        }

        private static (Button play, Button stats, Button settings, Button exit) CreateMainMenuButtons(Transform parent)
        {
            var panel = new GameObject("MenuPanel", typeof(RectTransform), typeof(Image), typeof(VerticalLayoutGroup));
            panel.transform.SetParent(parent, false);
            var rect = panel.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(760, 820);

            var img = panel.GetComponent<Image>();
            img.color = new Color(0.08f, 0.08f, 0.09f, 0.90f);

            var layout = panel.GetComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(36, 36, 36, 36);
            layout.spacing = 18;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlHeight = true;
            layout.childControlWidth = true;
            layout.childForceExpandHeight = false;
            layout.childForceExpandWidth = false;

            var play = CreateButton(panel.transform, "Play");
            var stats = CreateButton(panel.transform, "Stats");
            var settings = CreateButton(panel.transform, "Settings");
            var exit = CreateButton(panel.transform, "Exit");

            return (play, stats, settings, exit);
        }

        private static ThemeSelectionPopupController CreateThemeSelectionPopup(Transform parent, ThemeRegistry registry)
        {
            var popup = CreatePopupRoot(parent, "ThemeSelectionPopup");
            var panel = GetPopupPanel(popup.transform);
            var controller = popup.AddComponent<ThemeSelectionPopupController>();

            CreatePopupTitle(panel, "Select Theme");

            var container = new GameObject("ThemeButtons", typeof(RectTransform), typeof(VerticalLayoutGroup));
            container.transform.SetParent(panel, false);
            var v = container.GetComponent<VerticalLayoutGroup>();
            v.spacing = 12;
            v.childAlignment = TextAnchor.MiddleCenter;
            v.childControlHeight = true;
            v.childControlWidth = true;
            v.childForceExpandHeight = false;
            v.childForceExpandWidth = true;

            var buttonPrefabGo = new GameObject("ThemeButtonPrefab", typeof(RectTransform), typeof(Image), typeof(Button), typeof(LayoutElement));
            buttonPrefabGo.transform.SetParent(panel, false);
            var buttonPrefab = buttonPrefabGo.GetComponent<Button>();
            var text = CreateTMPText(buttonPrefabGo.transform, "Label", "Theme");
            text.alignment = TextAlignmentOptions.MidlineLeft;
            text.fontSize = 40;
            text.enableWordWrapping = false;
            var textRect = text.GetComponent<RectTransform>();
            textRect.anchorMin = new Vector2(0f, 0.5f);
            textRect.anchorMax = new Vector2(1f, 0.5f);
            textRect.pivot = new Vector2(0f, 0.5f);
            textRect.anchoredPosition = new Vector2(84f, 0f);
            textRect.sizeDelta = new Vector2(-110f, 60f);

            var dot = new GameObject("Dot", typeof(RectTransform), typeof(Image));
            dot.transform.SetParent(buttonPrefabGo.transform, false);
            var dotRect = dot.GetComponent<RectTransform>();
            dotRect.anchorMin = new Vector2(0f, 0.5f);
            dotRect.anchorMax = new Vector2(0f, 0.5f);
            dotRect.pivot = new Vector2(0f, 0.5f);
            dotRect.sizeDelta = new Vector2(18f, 18f);
            dotRect.anchoredPosition = new Vector2(44f, 0f);
            var dotImg = dot.GetComponent<Image>();
            dotImg.color = new Color(0.25f, 0.85f, 1f, 1f);
            dotImg.raycastTarget = false;

            var img = buttonPrefabGo.GetComponent<Image>();
            img.color = new Color(1f, 1f, 1f, 0.10f);

            var le = buttonPrefabGo.GetComponent<LayoutElement>();
            le.preferredWidth = 620;
            le.preferredHeight = 104;
            le.minHeight = 96;
            buttonPrefabGo.SetActive(false);

            var row = new GameObject("Buttons", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            row.transform.SetParent(panel, false);
            var h = row.GetComponent<HorizontalLayoutGroup>();
            h.spacing = 16;
            h.childAlignment = TextAnchor.MiddleCenter;
            h.childControlHeight = true;
            h.childControlWidth = true;
            h.childForceExpandHeight = false;
            h.childForceExpandWidth = false;

            var start = CreateButton(row.transform, "Start");
            var close = CreateButton(row.transform, "Close");
            start.GetComponent<Image>().color = new Color(0.25f, 0.85f, 1f, 0.22f);
            close.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.10f);

            SetPrivateField(controller, "root", popup);
            SetPrivateField(controller, "themeRegistry", registry);
            SetPrivateField(controller, "themeButtonContainer", container.transform);
            SetPrivateField(controller, "themeButtonPrefab", buttonPrefab);
            SetPrivateField(controller, "startButton", start);
            SetPrivateField(controller, "closeButton", close);
            SetPrivateField(controller, "animator", popup.GetComponent<PopupAnimator>());

            return controller;
        }

        private static StatsPopupController CreateStatsPopup(Transform parent)
        {
            var popup = CreatePopupRoot(parent, "StatsPopup");
            var panel = GetPopupPanel(popup.transform);
            var controller = popup.AddComponent<StatsPopupController>();

            var content = new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup));
            content.transform.SetParent(panel, false);
            var v = content.GetComponent<VerticalLayoutGroup>();
            v.spacing = 8;
            v.childAlignment = TextAnchor.UpperLeft;
            v.childControlHeight = true;
            v.childControlWidth = true;
            v.childForceExpandHeight = false;
            v.childForceExpandWidth = true;

            var title = CreatePopupTitle(panel, "Statistics");
            var total = CreatePopupLineText(content.transform, "TotalGames", "Total games played: 0");
            var p1 = CreatePopupLineText(content.transform, "P1Wins", "Player 1 wins: 0");
            var p2 = CreatePopupLineText(content.transform, "P2Wins", "Player 2 wins: 0");
            var draws = CreatePopupLineText(content.transform, "Draws", "Draws: 0");
            var avg = CreatePopupLineText(content.transform, "Avg", "Average game duration: 00:00");

            var close = CreateButton(panel, "Close");

            SetPrivateField(controller, "root", popup);
            SetPrivateField(controller, "totalGamesText", total);
            SetPrivateField(controller, "player1WinsText", p1);
            SetPrivateField(controller, "player2WinsText", p2);
            SetPrivateField(controller, "drawsText", draws);
            SetPrivateField(controller, "avgDurationText", avg);
            SetPrivateField(controller, "closeButton", close);
            SetPrivateField(controller, "animator", popup.GetComponent<PopupAnimator>());

            return controller;
        }

        private static SettingsPopupController CreateSettingsPopup(Transform parent)
        {
            var popup = CreatePopupRoot(parent, "SettingsPopup");
            var panel = GetPopupPanel(popup.transform);
            var controller = popup.AddComponent<SettingsPopupController>();

            CreatePopupTitle(panel, "Settings");
            var content = new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup));
            content.transform.SetParent(panel, false);
            var v = content.GetComponent<VerticalLayoutGroup>();
            v.spacing = 10;
            v.childAlignment = TextAnchor.UpperLeft;
            v.childControlHeight = true;
            v.childControlWidth = true;
            v.childForceExpandHeight = false;
            v.childForceExpandWidth = true;

            var bgm = CreateToggle(content.transform, "BGM");
            var sfx = CreateToggle(content.transform, "SFX");
            var close = CreateButton(panel, "Close");

            SetPrivateField(controller, "root", popup);
            SetPrivateField(controller, "bgmToggle", bgm);
            SetPrivateField(controller, "sfxToggle", sfx);
            SetPrivateField(controller, "closeButton", close);
            SetPrivateField(controller, "animator", popup.GetComponent<PopupAnimator>());

            return controller;
        }

        private static ExitConfirmationPopupController CreateExitPopup(Transform parent)
        {
            var popup = CreatePopupRoot(parent, "ExitPopup");
            var panel = GetPopupPanel(popup.transform);
            var controller = popup.AddComponent<ExitConfirmationPopupController>();

            var title = CreatePopupTitle(panel, "Exit?");
            title.alignment = TextAlignmentOptions.Center;

            var row = new GameObject("Buttons", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            row.transform.SetParent(panel, false);
            var h = row.GetComponent<HorizontalLayoutGroup>();
            h.spacing = 16;
            h.childAlignment = TextAnchor.MiddleCenter;

            var confirm = CreateButton(row.transform, "Confirm");
            var cancel = CreateButton(row.transform, "Cancel");

            SetPrivateField(controller, "root", popup);
            SetPrivateField(controller, "confirmButton", confirm);
            SetPrivateField(controller, "cancelButton", cancel);
            SetPrivateField(controller, "animator", popup.GetComponent<PopupAnimator>());

            return controller;
        }

        private static GameResultPopupController CreateGameResultPopup(Transform parent)
        {
            var popup = CreatePopupRoot(parent, "GameResultPopup");
            var panel = GetPopupPanel(popup.transform);
            var controller = popup.AddComponent<GameResultPopupController>();

            var title = CreatePopupTitle(panel, "Game Over");
            title.alignment = TextAlignmentOptions.Center;
            title.fontSize = 64;

            var duration = CreatePopupLineText(panel, "Duration", "Duration: 00:00");
            duration.alignment = TextAlignmentOptions.Center;
            duration.fontSize = 42;

            var row = new GameObject("Buttons", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            row.transform.SetParent(panel, false);
            var h = row.GetComponent<HorizontalLayoutGroup>();
            h.spacing = 16;
            h.childAlignment = TextAnchor.MiddleCenter;

            var retry = CreateButton(row.transform, "Retry");
            var exit = CreateButton(row.transform, "Exit");

            SetPrivateField(controller, "root", popup);
            SetPrivateField(controller, "animator", popup.GetComponent<PopupAnimator>());
            SetPrivateField(controller, "titleText", title);
            SetPrivateField(controller, "durationText", duration);
            SetPrivateField(controller, "retryButton", retry);
            SetPrivateField(controller, "exitButton", exit);

            return controller;
        }

        private static BoardCellView CreateBoardCell(Transform parent, int index)
        {
            var go = new GameObject($"Cell_{index}", typeof(RectTransform), typeof(Image), typeof(Button), typeof(BoardCellView));
            go.transform.SetParent(parent, false);

            var bg = go.GetComponent<Image>();
            bg.color = new Color(1f, 1f, 1f, 0.08f);

            var mark = new GameObject("Mark", typeof(RectTransform), typeof(Image));
            mark.transform.SetParent(go.transform, false);
            var markRect = mark.GetComponent<RectTransform>();
            markRect.anchorMin = Vector2.zero;
            markRect.anchorMax = Vector2.one;
            markRect.offsetMin = new Vector2(20, 20);
            markRect.offsetMax = new Vector2(-20, -20);

            var markText = CreateTMPText(go.transform, "MarkText", string.Empty);
            markText.alignment = TextAlignmentOptions.Center;
            markText.fontSize = 120;
            var mtRect = markText.GetComponent<RectTransform>();
            mtRect.anchorMin = Vector2.zero;
            mtRect.anchorMax = Vector2.one;
            mtRect.offsetMin = Vector2.zero;
            mtRect.offsetMax = Vector2.zero;
            markText.enabled = false;

            var view = go.GetComponent<BoardCellView>();
            SetPrivateField(view, "cellBackground", bg);
            SetPrivateField(view, "markImage", mark.GetComponent<Image>());
            SetPrivateField(view, "markText", markText);

            return view;
        }

        private static StrikeView CreateStrike(Transform parent)
        {
            // Important: parent has GridLayoutGroup for board cells. Strike must ignore that layout.
            var go = new GameObject("Strike", typeof(RectTransform), typeof(Image), typeof(LayoutElement), typeof(StrikeView));
            go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(600, 20);

            var img = go.GetComponent<Image>();
            img.color = new Color(1f, 1f, 1f, 0.6f);
            img.raycastTarget = false;

            var le = go.GetComponent<LayoutElement>();
            le.ignoreLayout = true;

            var strike = go.GetComponent<StrikeView>();
            SetPrivateField(strike, "strikeImage", img);
            SetPrivateField(strike, "strikeRect", rect);
            go.SetActive(false);
            return strike;
        }

        private static (HUDController hud, GameObject settingsButton) CreateHudWithSettings(Transform parent)
        {
            var hudGo = new GameObject("HUD", typeof(RectTransform));
            hudGo.transform.SetParent(parent, false);
            var rect = hudGo.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.sizeDelta = new Vector2(0, 180);
            rect.anchoredPosition = new Vector2(0, 0);

            var bg = hudGo.AddComponent<Image>();
            bg.color = new Color(0f, 0f, 0f, 0.25f);

            var layout = hudGo.AddComponent<HorizontalLayoutGroup>();
            layout.padding = new RectOffset(20, 20, 20, 20);
            layout.spacing = 20;
            layout.childAlignment = TextAnchor.MiddleCenter;

            var duration = CreateTMPText(hudGo.transform, "Duration", "00:00");
            var p1 = CreateTMPText(hudGo.transform, "P1Moves", "0");
            var p2 = CreateTMPText(hudGo.transform, "P2Moves", "0");
            var settingsButton = CreateButton(hudGo.transform, "Settings");
            settingsButton.GetComponent<RectTransform>().sizeDelta = new Vector2(260, 110);

            var hud = hudGo.AddComponent<HUDController>();
            SetPrivateField(hud, "durationText", duration);
            SetPrivateField(hud, "player1MovesText", p1);
            SetPrivateField(hud, "player2MovesText", p2);

            return (hud, settingsButton.gameObject);
        }

        private static GameObject CreatePopupRoot(Transform parent, string name)
        {
            var overlay = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(CanvasGroup), typeof(PopupAnimator));
            overlay.transform.SetParent(parent, false);
            var rect = overlay.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            var img = overlay.GetComponent<Image>();
            img.color = new Color(0f, 0f, 0f, 0.62f);

            var panel = new GameObject("Panel", typeof(RectTransform), typeof(Image), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
            panel.transform.SetParent(overlay.transform, false);
            var pRect = panel.GetComponent<RectTransform>();
            pRect.anchorMin = new Vector2(0.5f, 0.5f);
            pRect.anchorMax = new Vector2(0.5f, 0.5f);
            pRect.pivot = new Vector2(0.5f, 0.5f);
            pRect.sizeDelta = new Vector2(860, 720);
            pRect.anchoredPosition = Vector2.zero;

            var pImg = panel.GetComponent<Image>();
            pImg.color = new Color(0.08f, 0.08f, 0.09f, 0.95f);

            var v = panel.GetComponent<VerticalLayoutGroup>();
            v.padding = new RectOffset(40, 40, 40, 40);
            v.spacing = 18;
            v.childAlignment = TextAnchor.UpperCenter;
            v.childControlHeight = true;
            v.childControlWidth = true;

            var fitter = panel.GetComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var animator = overlay.GetComponent<PopupAnimator>();
            SetPrivateField(animator, "panel", panel.GetComponent<RectTransform>());

            overlay.SetActive(false);
            return overlay;
        }

        private static Transform GetPopupPanel(Transform overlay)
        {
            var t = overlay.Find("Panel");
            return t != null ? t : overlay;
        }

        private static Button CreateButton(Transform parent, string label)
        {
            var go = new GameObject($"Button_{label}", typeof(RectTransform), typeof(Image), typeof(Button), typeof(LayoutElement));
            go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(0, 0);

            var img = go.GetComponent<Image>();
            img.color = new Color(1f, 1f, 1f, 0.10f);

            var le = go.GetComponent<LayoutElement>();
            le.preferredWidth = 620;
            le.preferredHeight = 120;
            le.minWidth = 420;
            le.minHeight = 110;

            var text = CreateTMPText(go.transform, "Label", label);
            text.alignment = TextAlignmentOptions.Center;
            text.fontSize = 40;

            return go.GetComponent<Button>();
        }

        private static Toggle CreateToggle(Transform parent, string label)
        {
            var root = new GameObject($"Toggle_{label}", typeof(RectTransform), typeof(Toggle), typeof(LayoutElement));
            root.transform.SetParent(parent, false);
            var rect = root.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(0, 0);

            var le = root.GetComponent<LayoutElement>();
            le.preferredHeight = 84;
            le.minHeight = 72;

            var background = new GameObject("Background", typeof(RectTransform), typeof(Image));
            background.transform.SetParent(root.transform, false);
            var bgRect = background.GetComponent<RectTransform>();
            bgRect.anchorMin = new Vector2(1f, 0.5f);
            bgRect.anchorMax = new Vector2(1f, 0.5f);
            bgRect.pivot = new Vector2(1f, 0.5f);
            bgRect.sizeDelta = new Vector2(64, 64);
            bgRect.anchoredPosition = new Vector2(-10, 0);
            var bgImg = background.GetComponent<Image>();
            bgImg.color = new Color(1f, 1f, 1f, 0.12f);

            var checkmark = new GameObject("Checkmark", typeof(RectTransform), typeof(Image));
            checkmark.transform.SetParent(background.transform, false);
            var cmRect = checkmark.GetComponent<RectTransform>();
            cmRect.anchorMin = Vector2.zero;
            cmRect.anchorMax = Vector2.one;
            cmRect.offsetMin = new Vector2(10, 10);
            cmRect.offsetMax = new Vector2(-10, -10);
            var cmImg = checkmark.GetComponent<Image>();
            cmImg.color = new Color(1f, 1f, 1f, 0.90f);

            var labelText = CreateTMPText(root.transform, "Label", label);
            var labelRect = labelText.GetComponent<RectTransform>();
            labelRect.anchorMin = new Vector2(0f, 0.5f);
            labelRect.anchorMax = new Vector2(1f, 0.5f);
            labelRect.pivot = new Vector2(0f, 0.5f);
            labelRect.anchoredPosition = new Vector2(10, 0);
            labelRect.sizeDelta = new Vector2(-110, 60);
            labelText.fontSize = 36;
            labelText.enableWordWrapping = false;

            var toggle = root.GetComponent<Toggle>();
            toggle.targetGraphic = bgImg;
            toggle.graphic = cmImg;

            toggle.isOn = true;
            toggle.graphic.enabled = true;

            return toggle;
        }

        private static TMP_Text CreateTMPText(Transform parent, string name, string text)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            go.transform.SetParent(parent, false);
            var tmp = go.GetComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = 40;
            tmp.color = Color.white;
            return tmp;
        }

        private static TMP_Text CreatePopupTitle(Transform parent, string text)
        {
            var tmp = CreateTMPText(parent, "Title", text);
            tmp.fontSize = 56;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.enableWordWrapping = false;
            var rt = tmp.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(0, 90);
            var le = tmp.gameObject.AddComponent<LayoutElement>();
            le.preferredHeight = 90;
            le.minHeight = 80;
            return tmp;
        }

        private static TMP_Text CreatePopupLineText(Transform parent, string name, string text)
        {
            var tmp = CreateTMPText(parent, name, text);
            tmp.fontSize = 34;
            tmp.alignment = TextAlignmentOptions.Left;
            tmp.enableWordWrapping = false;
            var rt = tmp.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(0, 56);
            var le = tmp.gameObject.AddComponent<LayoutElement>();
            le.preferredHeight = 56;
            le.minHeight = 48;
            return tmp;
        }

        private static void SetPrivateField(Object target, string fieldName, object value)
        {
            var so = new SerializedObject(target);
            var prop = so.FindProperty(fieldName);
            if (prop == null)
            {
                Debug.LogWarning($"TicTacToe: Missing field '{fieldName}' on {target.GetType().Name}");
                return;
            }

            switch (prop.propertyType)
            {
                case SerializedPropertyType.ObjectReference:
                    prop.objectReferenceValue = value as Object;
                    break;
                case SerializedPropertyType.Generic:
                    if (value is IList<ThemeDefinition> themes && prop.isArray)
                    {
                        prop.arraySize = themes.Count;
                        for (var i = 0; i < themes.Count; i++)
                            prop.GetArrayElementAtIndex(i).objectReferenceValue = themes[i];
                    }
                    else if (value is BoardCellView[] cells && prop.isArray)
                    {
                        prop.arraySize = cells.Length;
                        for (var i = 0; i < cells.Length; i++)
                            prop.GetArrayElementAtIndex(i).objectReferenceValue = cells[i];
                    }
                    else if (value is Object[] objects && prop.isArray)
                    {
                        // Generic handler for serialized Object reference arrays (e.g. AudioClip[]).
                        prop.arraySize = objects.Length;
                        for (var i = 0; i < objects.Length; i++)
                            prop.GetArrayElementAtIndex(i).objectReferenceValue = objects[i];
                    }
                    break;
            }

            so.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}

