using System;
using System.Collections.Generic;
using UnityEngine;

namespace TicTacToe.Audio
{
    public sealed class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [Header("Sources")]
        [SerializeField] private AudioSource bgmSource;
        [SerializeField] private AudioSource sfxSource;

        [Header("Clips")]
        [SerializeField] private AudioClip bgmClip;
        [SerializeField] private AudioClip[] buttonClickClips;
        [SerializeField] private AudioClip placeMarkClip;
        [SerializeField] private AudioClip strikeWinClip;
        [SerializeField] private AudioClip popupOpenClip;
        [SerializeField] private AudioClip popupCloseClip;

        private readonly AudioSettingsService _settingsService = new AudioSettingsService();
        private AudioSettingsModel _settings;

        private Dictionary<AudioEvent, Func<AudioClip>> _clipMap;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            _settings = _settingsService.Load();
            _clipMap = new Dictionary<AudioEvent, Func<AudioClip>>
            {
                { AudioEvent.ButtonClick, PickButtonClickClip },
                { AudioEvent.PlaceMark, () => placeMarkClip },
                { AudioEvent.StrikeWin, () => strikeWinClip },
                { AudioEvent.PopupOpen, () => popupOpenClip },
                { AudioEvent.PopupClose, () => popupCloseClip }
            };

            ApplySettings();
        }

        public AudioSettingsModel GetSettings() => new AudioSettingsModel
        {
            bgmEnabled = _settings.bgmEnabled,
            sfxEnabled = _settings.sfxEnabled
        };

        public void SetBgmEnabled(bool enabled)
        {
            _settings.bgmEnabled = enabled;
            _settingsService.Save(_settings);
            ApplySettings();
        }

        public void SetSfxEnabled(bool enabled)
        {
            _settings.sfxEnabled = enabled;
            _settingsService.Save(_settings);
            ApplySettings();
        }

        public void Play(AudioEvent audioEvent)
        {
            if (!_settings.sfxEnabled || sfxSource == null)
                return;

            if (!_clipMap.TryGetValue(audioEvent, out var picker) || picker == null)
                return;

            var clip = picker();
            if (clip == null)
                return;

            sfxSource.PlayOneShot(clip);
        }

        private AudioClip PickButtonClickClip()
        {
            if (buttonClickClips == null || buttonClickClips.Length == 0)
                return null;
            if (buttonClickClips.Length == 1)
                return buttonClickClips[0];
            return buttonClickClips[UnityEngine.Random.Range(0, buttonClickClips.Length)];
        }

        private void ApplySettings()
        {
            if (bgmSource != null)
            {
                bgmSource.loop = true;
                bgmSource.clip = bgmClip;
                bgmSource.mute = !_settings.bgmEnabled;
                if (_settings.bgmEnabled && bgmClip != null && !bgmSource.isPlaying)
                    bgmSource.Play();
            }

            if (sfxSource != null)
                sfxSource.mute = !_settings.sfxEnabled;
        }
    }
}

