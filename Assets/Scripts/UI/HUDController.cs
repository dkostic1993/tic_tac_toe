using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TicTacToe.Game
{
    public sealed class HUDController : MonoBehaviour
    {
        [SerializeField] private TMP_Text durationText;
        [SerializeField] private TMP_Text player1MovesText;
        [SerializeField] private TMP_Text player2MovesText;
        [SerializeField] private Image player1MoveFrame;
        [SerializeField] private Image player2MoveFrame;

        private static readonly Color InactiveFrameColor = new Color(1f, 1f, 1f, 0.07f);
        private static readonly Color IdleTextColor = new Color(1f, 1f, 1f, 0.9f);

        public void SetDurationSeconds(float seconds)
        {
            if (durationText == null)
                return;
            var s = Mathf.Max(0f, seconds);
            durationText.text = $"Time: {FormatDuration(s)}";
        }

        public void SetMoveCounts(int p1, int p2, PlayerMark currentTurn, bool gameOver, Color xColor, Color oColor)
        {
            if (player1MovesText != null)
                player1MovesText.text = $"P1: {p1}";
            if (player2MovesText != null)
                player2MovesText.text = $"P2: {p2}";

            if (gameOver)
            {
                ApplyTurnVisuals(player1MoveFrame, player1MovesText, active: false, markColor: xColor);
                ApplyTurnVisuals(player2MoveFrame, player2MovesText, active: false, markColor: oColor);
                return;
            }

            ApplyTurnVisuals(player1MoveFrame, player1MovesText, currentTurn == PlayerMark.X, xColor);
            ApplyTurnVisuals(player2MoveFrame, player2MovesText, currentTurn == PlayerMark.O, oColor);
        }

        private static void ApplyTurnVisuals(Image frame, TMP_Text text, bool active, Color markColor)
        {
            if (frame != null)
            {
                frame.color = active
                    ? new Color(markColor.r, markColor.g, markColor.b, 0.48f)
                    : InactiveFrameColor;
            }

            if (text != null)
                text.color = active ? markColor : IdleTextColor;
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

