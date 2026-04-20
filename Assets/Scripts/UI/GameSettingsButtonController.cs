using TicTacToe.Audio;
using UnityEngine;
using UnityEngine.UI;

namespace TicTacToe.UI
{
    public sealed class GameSettingsButtonController : MonoBehaviour
    {
        [SerializeField] private Button settingsButton;
        [SerializeField] private SettingsPopupController settingsPopup;

        private void Awake()
        {
            if (settingsButton != null)
                settingsButton.onClick.AddListener(() =>
                {
                    AudioManager.Instance?.Play(AudioEvent.ButtonClick);
                    settingsPopup?.Show();
                });
        }

        private void Start()
        {
            settingsPopup?.HideImmediate();
        }
    }
}

