using TicTacToe.Audio;
using UnityEngine;
using UnityEngine.UI;

namespace TicTacToe.UI
{
    public sealed class PlayMenuController : MonoBehaviour
    {
        [SerializeField] private Button playButton;
        [SerializeField] private Button statsButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button exitButton;

        [Header("Popups")]
        [SerializeField] private ThemeSelectionPopupController themeSelectionPopup;
        [SerializeField] private StatsPopupController statsPopup;
        [SerializeField] private SettingsPopupController settingsPopup;
        [SerializeField] private ExitConfirmationPopupController exitPopup;

        private void Awake()
        {
            if (playButton != null) playButton.onClick.AddListener(OnPlay);
            if (statsButton != null) statsButton.onClick.AddListener(OnStats);
            if (settingsButton != null) settingsButton.onClick.AddListener(OnSettings);
            if (exitButton != null) exitButton.onClick.AddListener(OnExit);
        }

        private void Start()
        {
            themeSelectionPopup?.HideImmediate();
            statsPopup?.HideImmediate();
            settingsPopup?.HideImmediate();
            exitPopup?.HideImmediate();
        }

        private void OnPlay()
        {
            AudioManager.Instance?.Play(AudioEvent.ButtonClick);
            themeSelectionPopup?.Show();
        }

        private void OnStats()
        {
            AudioManager.Instance?.Play(AudioEvent.ButtonClick);
            statsPopup?.Show();
        }

        private void OnSettings()
        {
            AudioManager.Instance?.Play(AudioEvent.ButtonClick);
            settingsPopup?.Show();
        }

        private void OnExit()
        {
            AudioManager.Instance?.Play(AudioEvent.ButtonClick);
            exitPopup?.Show();
        }
    }
}

