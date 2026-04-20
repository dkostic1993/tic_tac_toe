using UnityEngine;

namespace TicTacToe.Audio
{
    public sealed class AudioSettingsService
    {
        private const string PlayerPrefsKey = "tictactoe.audioSettings.v1";

        public AudioSettingsModel Load()
        {
            var json = PlayerPrefs.GetString(PlayerPrefsKey, string.Empty);
            if (string.IsNullOrWhiteSpace(json))
                return new AudioSettingsModel();

            try
            {
                var loaded = JsonUtility.FromJson<AudioSettingsModel>(json);
                return loaded ?? new AudioSettingsModel();
            }
            catch
            {
                return new AudioSettingsModel();
            }
        }

        public void Save(AudioSettingsModel model)
        {
            var json = JsonUtility.ToJson(model);
            PlayerPrefs.SetString(PlayerPrefsKey, json);
            PlayerPrefs.Save();
        }
    }
}

