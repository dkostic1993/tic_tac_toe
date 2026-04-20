using TicTacToe.Audio;
using UnityEngine;
using UnityEngine.UI;

namespace TicTacToe.UI
{
    public sealed class ExitConfirmationPopupController : PopupBase
    {
        [SerializeField] private Button confirmButton;
        [SerializeField] private Button cancelButton;

        private void Awake()
        {
            if (confirmButton != null) confirmButton.onClick.AddListener(OnConfirm);
            if (cancelButton != null) cancelButton.onClick.AddListener(OnCancel);
        }

        private void OnConfirm()
        {
            AudioManager.Instance?.Play(AudioEvent.ButtonClick);
#if UNITY_WEBGL
            Hide();
#elif UNITY_EDITOR
            // Application.Quit() does nothing in the Unity Editor.
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private void OnCancel()
        {
            AudioManager.Instance?.Play(AudioEvent.ButtonClick);
            Hide();
        }
    }
}

