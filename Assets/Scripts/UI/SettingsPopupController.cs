using TicTacToe.Audio;
using UnityEngine;
using UnityEngine.UI;

namespace TicTacToe.UI
{
    public sealed class SettingsPopupController : PopupBase
    {
        [SerializeField] private Toggle bgmToggle;
        [SerializeField] private Toggle sfxToggle;
        [SerializeField] private Button closeButton;

        private bool _isBinding;

        private void Awake()
        {
            if (bgmToggle != null) bgmToggle.onValueChanged.AddListener(OnBgmChanged);
            if (sfxToggle != null) sfxToggle.onValueChanged.AddListener(OnSfxChanged);
            if (closeButton != null) closeButton.onClick.AddListener(OnClose);
        }

        public override void Show()
        {
            BindFromManager();
            base.Show();
        }

        private void BindFromManager()
        {
            var mgr = AudioManager.Instance;
            if (mgr == null)
                return;

            _isBinding = true;
            var settings = mgr.GetSettings();
            if (bgmToggle != null) bgmToggle.isOn = settings.bgmEnabled;
            if (sfxToggle != null) sfxToggle.isOn = settings.sfxEnabled;
            _isBinding = false;
        }

        private void OnBgmChanged(bool enabled)
        {
            if (_isBinding) return;
            AudioManager.Instance?.SetBgmEnabled(enabled);
        }

        private void OnSfxChanged(bool enabled)
        {
            if (_isBinding) return;
            AudioManager.Instance?.SetSfxEnabled(enabled);
        }

        private void OnClose()
        {
            AudioManager.Instance?.Play(AudioEvent.ButtonClick);
            Hide();
        }
    }
}

