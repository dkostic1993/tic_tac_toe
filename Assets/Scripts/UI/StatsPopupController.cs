using TMPro;
using TicTacToe.Audio;
using TicTacToe.Stats;
using UnityEngine;
using UnityEngine.UI;

namespace TicTacToe.UI
{
    public sealed class StatsPopupController : PopupBase
    {
        [SerializeField] private TMP_Text totalGamesText;
        [SerializeField] private TMP_Text player1WinsText;
        [SerializeField] private TMP_Text player2WinsText;
        [SerializeField] private TMP_Text drawsText;
        [SerializeField] private TMP_Text avgDurationText;
        [SerializeField] private Button closeButton;

        private readonly StatsService _statsService = new StatsService();

        private void Awake()
        {
            if (closeButton != null)
                closeButton.onClick.AddListener(() =>
                {
                    AudioManager.Instance?.Play(AudioEvent.ButtonClick);
                    Hide();
                });
        }

        public override void Show()
        {
            var model = _statsService.Load();
            if (totalGamesText != null) totalGamesText.text = $"Total games played: {model.totalGames}";
            if (player1WinsText != null) player1WinsText.text = $"Player 1 wins: {model.player1Wins}";
            if (player2WinsText != null) player2WinsText.text = $"Player 2 wins: {model.player2Wins}";
            if (drawsText != null) drawsText.text = $"Draws: {model.draws}";
            if (avgDurationText != null) avgDurationText.text = $"Average game duration: {FormatDuration(model.AverageDurationSeconds)}";

            base.Show();
        }

        private static string FormatDuration(float seconds)
        {
            var total = Mathf.FloorToInt(Mathf.Max(0f, seconds));
            var min = total / 60;
            var sec = total % 60;
            return $"{min:00}:{sec:00}";
        }
    }
}

