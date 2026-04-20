using UnityEngine;

namespace TicTacToe.Stats
{
    public sealed class StatsService
    {
        private const string PlayerPrefsKey = "tictactoe.stats.v1";

        public StatsModel Load()
        {
            var json = PlayerPrefs.GetString(PlayerPrefsKey, string.Empty);
            if (string.IsNullOrWhiteSpace(json))
                return new StatsModel();

            try
            {
                var loaded = JsonUtility.FromJson<StatsModel>(json);
                return loaded ?? new StatsModel();
            }
            catch
            {
                return new StatsModel();
            }
        }

        public void Save(StatsModel model)
        {
            var json = JsonUtility.ToJson(model);
            PlayerPrefs.SetString(PlayerPrefsKey, json);
            PlayerPrefs.Save();
        }
    }
}

