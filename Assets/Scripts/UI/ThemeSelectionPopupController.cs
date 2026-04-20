using System.Collections.Generic;
using TicTacToe.Audio;
using TicTacToe.Game;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

namespace TicTacToe.UI
{
    public sealed class ThemeSelectionPopupController : PopupBase
    {
        [SerializeField] private ThemeRegistry themeRegistry;
        [SerializeField] private Transform themeButtonContainer;
        [SerializeField] private Button themeButtonPrefab;
        [SerializeField] private Button startButton;
        [SerializeField] private Button closeButton;

        private readonly ThemeService _themeService = new ThemeService();
        private readonly List<Button> _spawned = new List<Button>();

        private string _selectedThemeName;

        private void Awake()
        {
            if (startButton != null) startButton.onClick.AddListener(OnStart);
            if (closeButton != null) closeButton.onClick.AddListener(OnClose);
        }

        public override void Show()
        {
            RebuildButtons();
            base.Show();
        }

        private void RebuildButtons()
        {
            foreach (var b in _spawned)
            {
                if (b != null) Destroy(b.gameObject);
            }
            _spawned.Clear();

            if (themeRegistry == null || themeButtonContainer == null || themeButtonPrefab == null)
                return;

            var current = _themeService.LoadThemeName();
            _selectedThemeName = current;

            foreach (var theme in themeRegistry.Themes)
            {
                if (theme == null) continue;
                var button = Instantiate(themeButtonPrefab, themeButtonContainer);
                // If prefab is inactive (common for UI templates), ensure spawned instances are interactable.
                button.gameObject.SetActive(true);
                _spawned.Add(button);

                var text = button.GetComponentInChildren<TMP_Text>();
                if (text != null) text.text = theme.themeName;

                var themeName = theme.themeName;
                button.onClick.AddListener(() => OnThemeSelected(themeName));
            }

            RefreshSelectionVisuals();
        }

        private void OnThemeSelected(string themeName)
        {
            AudioManager.Instance?.Play(AudioEvent.ButtonClick);
            _selectedThemeName = themeName;
            _themeService.SaveThemeName(themeName);
            RefreshSelectionVisuals();
        }

        private void RefreshSelectionVisuals()
        {
            foreach (var b in _spawned)
            {
                if (b == null) continue;
                var text = b.GetComponentInChildren<TMP_Text>();
                var isSelected = text != null && string.Equals(text.text, _selectedThemeName);
                var img = b.GetComponent<Image>();
                if (img != null)
                    img.color = isSelected ? new Color(1f, 1f, 1f, 0.22f) : new Color(1f, 1f, 1f, 0.10f);

                // Optional preview dot (created by generator)
                var dot = b.transform.Find("Dot")?.GetComponent<Image>();
                if (dot != null)
                    dot.color = isSelected ? new Color(0.25f, 0.85f, 1f, 1f) : new Color(0.7f, 0.7f, 0.7f, 0.75f);
            }
        }

        private void OnStart()
        {
            AudioManager.Instance?.Play(AudioEvent.ButtonClick);
            SceneManager.LoadScene(SceneNames.Game);
        }

        private void OnClose()
        {
            AudioManager.Instance?.Play(AudioEvent.ButtonClick);
            Hide();
        }
    }
}

