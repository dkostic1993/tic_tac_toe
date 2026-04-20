using TMPro;
using UnityEngine;

namespace TicTacToe.Game
{
    public sealed class HUDController : MonoBehaviour
    {
        [SerializeField] private TMP_Text durationText;
        [SerializeField] private TMP_Text player1MovesText;
        [SerializeField] private TMP_Text player2MovesText;

        public void SetDurationSeconds(float seconds)
        {
            if (durationText == null)
                return;
            var s = Mathf.Max(0f, seconds);
            durationText.text = $"Time: {FormatDuration(s)}";
        }

        public void SetMoveCounts(int p1, int p2)
        {
            if (player1MovesText != null) player1MovesText.text = $"P1: {p1}";
            if (player2MovesText != null) player2MovesText.text = $"P2: {p2}";
        }

        private static string FormatDuration(float seconds)
        {
            var total = Mathf.FloorToInt(seconds);
            var min = total / 60;
            var sec = total % 60;
            return $"{min:00}:{sec:00}";
        }
    }
}

